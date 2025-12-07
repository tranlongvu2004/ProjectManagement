using Microsoft.AspNetCore.Mvc;
using PorjectManagement.Models;
using PorjectManagement.Service.Interface;
using PorjectManagement.ViewModels;

namespace PorjectManagement.Controllers
{
    public class ProjectManagementController : BaseController
    {
        private readonly IProjectServices _projectServices;
        private readonly IUserServices _userServices;

        public ProjectManagementController(
            IProjectServices projectServices,
            IUserServices userServices)
        {
            _projectServices = projectServices;
            _userServices = userServices;
        }

        // GET: /ProjectManagement/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var redirect = RedirectIfNotLoggedIn();
            if (redirect != null) return redirect;

            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail))
            {
                TempData["Error"] = "Không tìm thấy thông tin đăng nhập.";
                return RedirectToAction("Index", "Project");
            }
            var currentUser = _userServices.GetUser(userEmail);

            if (currentUser == null || currentUser.RoleId != 1)
            {
                TempData["Error"] = "Chỉ Mentor mới có quyền tạo project.";
                return RedirectToAction("Index", "Project");
            }

            var model = new ProjectCreateViewModel
            {
                Deadline = DateTime.Now.AddMonths(1),
                AvailableUsers = await _projectServices.GetAvailableUsersAsync()
            };

            return View(model);
        }

        // POST: /ProjectManagement/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProjectCreateViewModel model)
        {
            var redirect = RedirectIfNotLoggedIn();
            if (redirect != null) return redirect;

            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail))
            {
                TempData["Error"] = "Không tìm thấy thông tin đăng nhập.";
                return RedirectToAction("Index", "Project");
            }
            var currentUser = _userServices.GetUser(userEmail);

            if (currentUser == null || currentUser.RoleId != 1)
            {
                TempData["Error"] = "Không có quyền tạo project.";
                return RedirectToAction("Index", "Project");
            }

            if (!ModelState.IsValid)
            {
                model.AvailableUsers = await _projectServices.GetAvailableUsersAsync();
                return View(model);
            }

            if (model.SelectedUserIds == null || !model.SelectedUserIds.Any())
            {
                ModelState.AddModelError("", "Vui lòng chọn ít nhất 1 thành viên cho project.");
                model.AvailableUsers = await _projectServices.GetAvailableUsersAsync();
                return View(model);
            }

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
                return RedirectToAction("Details", "Workspace", new { id = projectId });
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