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
            TempData.Remove("Error");
            TempData.Remove("Success");

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

            if (model.Deadline.Date < DateTime.Now.Date)
            {
                ModelState.AddModelError("Deadline", "Deadline không được ở quá khứ");
            }
            
            if (model.SelectedUserIds == null || !model.SelectedUserIds.Any())
            {
                ModelState.AddModelError("SelectedUserIds", "Vui lòng chọn ít nhất 1 thành viên cho project.");
            }

            if (!model.LeaderId.HasValue || model.LeaderId.Value <= 0)
            {
                ModelState.AddModelError("LeaderId", "Vui lòng chọn Leader cho dự án.");
            }

            else if (model.SelectedUserIds != null && !model.SelectedUserIds.Contains(model.LeaderId.Value))
            {
                ModelState.AddModelError("LeaderId", "Leader phải là thành viên của project.");
            }

            if (!ModelState.IsValid)
            {
                model.AvailableUsers = await _projectServices.GetAvailableUsersAsync();
                return View(model);
            }

            try
            {
                int projectId = await _projectServices.CreateProjectWithTeamAsync(model, currentUser.UserId);
               
                TempData.Remove("Error");
                TempData["Success"] = "Tạo project thành công!";
                
                return RedirectToAction("Details", "Workspace", new { id = projectId });
            }
            catch (Exception ex)
            {
                TempData.Remove("Success");
                ModelState.AddModelError("", $"Lỗi khi tạo project: {ex.Message}");
                model.AvailableUsers = await _projectServices.GetAvailableUsersAsync();
                return View(model);
            }
        }

        // GET: /ProjectManagement/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
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
                TempData["Error"] = "Chỉ Mentor mới có quyền cập nhật project.";
                return RedirectToAction("Index", "Project");
            }

            var model = await _projectServices.GetProjectForUpdateAsync(id, currentUser.UserId);
            
            if (model == null)
            {
                TempData["Error"] = "Không tìm thấy project hoặc bạn không có quyền chỉnh sửa.";
                return RedirectToAction("Index", "Project");
            }

            TempData.Remove("Error");
            TempData.Remove("Success");

            return View(model);
        }

        // POST: /ProjectManagement/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProjectUpdateViewModel model)
        {
            var redirect = RedirectIfNotLoggedIn();
            if (redirect != null) return redirect;

            if (id != model.ProjectId)
            {
                TempData["Error"] = "Dữ liệu không hợp lệ.";
                return RedirectToAction("Index", "Project");
            }

            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail))
            {
                TempData["Error"] = "Không tìm thấy thông tin đăng nhập.";
                return RedirectToAction("Index", "Project");
            }

            var currentUser = _userServices.GetUser(userEmail);
            if (currentUser == null || currentUser.RoleId != 1)
            {
                TempData["Error"] = "Không có quyền cập nhật project.";
                return RedirectToAction("Index", "Project");
            }

            if (model.Deadline.Date < DateTime.Now.Date)
            {
                ModelState.AddModelError("Deadline", "Deadline không được ở quá khứ");
            }

            if (model.SelectedUserIds == null || !model.SelectedUserIds.Any())
            {
                ModelState.AddModelError("SelectedUserIds", "Vui lòng chọn ít nhất 1 thành viên cho project.");
            }

            if (!model.LeaderId.HasValue || model.LeaderId.Value <= 0)
            {
                ModelState.AddModelError("LeaderId", "Vui lòng chọn Leader cho dự án.");
            }
            else if (model.SelectedUserIds != null && !model.SelectedUserIds.Contains(model.LeaderId.Value))
            {
                ModelState.AddModelError("LeaderId", "Leader phải là thành viên của project.");
            }

            if (!ModelState.IsValid)
            {
                model.AvailableUsers = await _projectServices.GetAvailableUsersAsync();
                model.CurrentMembers = await _projectServices.GetProjectMembersAsync(id);
                return View(model);
            }

            try
            {
                bool success = await _projectServices.UpdateProjectWithTeamAsync(model, currentUser.UserId);
                
                if (!success)
                {
                    TempData["Error"] = "Không thể cập nhật project.";
                    return RedirectToAction("Index", "Project");
                }

                TempData.Remove("Error");
                TempData["Success"] = "Cập nhật project thành công!";
                
                return RedirectToAction("Details", "Workspace", new { id = model.ProjectId });
            }
            catch (Exception ex)
            {
                TempData.Remove("Success");
                ModelState.AddModelError("", $"Lỗi khi cập nhật project: {ex.Message}");
                model.AvailableUsers = await _projectServices.GetAvailableUsersAsync();
                model.CurrentMembers = await _projectServices.GetProjectMembersAsync(id);
                return View(model);
            }
        }
    }
}