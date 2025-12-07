using Microsoft.AspNetCore.Mvc;
using PorjectManagement.Models;
using PorjectManagement.Service.Interface;
using PorjectManagement.ViewModels;

namespace PorjectManagement.Controllers
{
    public class ProjectController : Controller
    {
        private readonly IProjectServices _projectServices;
        private readonly IUserServices _userServices;

        public ProjectController(IProjectServices projectServices, IUserServices userServices)
        {
            _projectServices = projectServices;
            _userServices = userServices;
        }

        // GET: /Project
        // Trang Project List: tạm thời lấy tất cả project
        public async Task<IActionResult> Index()
        {
            // TODO: sau này filter theo user đang đăng nhập
            List<Project> projects = await _projectServices.GetAllProjectsAsync();
            return View(projects);   // View: Views/Project/Index.cshtml
        }

        // GET: /Project/Workspace/5
        // Trang chi tiết Workspace của 1 project
        public async Task<IActionResult> Workspace(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Project id không hợp lệ.");
            }

            ProjectWorkspaceViewModel? model = await _projectServices.GetWorkspaceAsync(id);
            if (model == null)
            {
                return NotFound(); // Không tìm thấy project
            }

            return View(model);     // View: Views/Project/Workspace.cshtml
        }

        // NEW: Create Project
        // GET: /Project/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            // Kiểm tra user đã login chưa
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToAction("Login", "User");
            }

            var currentUser = _userServices.GetUser(userEmail);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "User");
            }

            // Kiểm tra quyền Mentor (RoleId = 1)
            if (currentUser.RoleId != 1)
            {
                TempData["Error"] = "Chỉ Mentor mới có quyền tạo project.";
                return RedirectToAction("Index");
            }

            // Load danh sách users để assign
            var model = new ProjectCreateViewModel
            {
                Deadline = DateTime.Now.AddMonths(1), // Default deadline
                AvailableUsers = await _projectServices.GetAvailableUsersAsync()
            };

            return View(model);
        }

        // POST: /Project/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProjectCreateViewModel model)
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToAction("Login", "User");
            }

            var currentUser = _userServices.GetUser(userEmail);
            if (currentUser == null || currentUser.RoleId != 1)
            {
                TempData["Error"] = "Không có quyền tạo project.";
                return RedirectToAction("Index");
            }

            if (!ModelState.IsValid)
            {
                // Reload available users nếu validation fail
                model.AvailableUsers = await _projectServices.GetAvailableUsersAsync();
                return View(model);
            }

            // Validate: phải chọn ít nhất 1 member
            if (model.SelectedUserIds == null || !model.SelectedUserIds.Any())
            {
                ModelState.AddModelError("", "Vui lòng chọn ít nhất 1 thành viên cho project.");
                model.AvailableUsers = await _projectServices.GetAvailableUsersAsync();
                return View(model);
            }

            // Validate: Leader phải nằm trong danh sách selected members
            if (model.LeaderId.HasValue && !model.SelectedUserIds.Contains(model.LeaderId.Value))
            {
                ModelState.AddModelError("", "Leader phải là thành viên của project.");
                model.AvailableUsers = await _projectServices.GetAvailableUsersAsync();
                return View(model);
            }

            try
            {
                int projectId = await _projectServices.CreateProjectWithTeamAsync(model, currentUser.UserId);
                TempData["Success"] = "Tạo project thành công!";
                return RedirectToAction("Workspace", new { id = projectId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Lỗi khi tạo project: {ex.Message}");
                model.AvailableUsers = await _projectServices.GetAvailableUsersAsync();
                return View(model);
            }
        }
    }
}
