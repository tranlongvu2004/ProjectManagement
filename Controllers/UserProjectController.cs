// Controllers/UserProjectController.cs
using Microsoft.AspNetCore.Mvc;
using PorjectManagement.Service.Interface;
using PorjectManagement.ViewModels;
using System.Linq;
using System.Threading.Tasks;

namespace PorjectManagement.Controllers
{
    public class UserProjectController : Controller
    {
        private readonly IUserProjectService _userProjectService;

        public UserProjectController(IUserProjectService userProjectService)
        {
            _userProjectService = userProjectService;
        }

        [HttpGet]
        public async Task<IActionResult> AddMembers(int? id)
        {
            var allProjects = await _userProjectService.GetAllProjectsAsync(); // NEW

            if (id == null)
            {
                return View(new AddMembersViewModel
                {
                    ProjectId = 0,
                    ProjectName = "Chưa chọn",
                    Users = new List<UserListItemVM>(),
                    AllProjects = allProjects
                });
            }

            var project = await _userProjectService.GetProjectByIdAsync(id.Value);
            if (project == null) return NotFound();

            var users = await _userProjectService.GetAllUsersAsync();

            var vm = new AddMembersViewModel
            {
                ProjectId = id.Value,
                ProjectName = project.ProjectName,
                Users = users.Select(u => new UserListItemVM
                {
                    UserId = u.UserId,
                    FullName = u.FullName,
                    Email = u.Email,
                    RoleName = u.Role?.RoleName ?? "",
                    CreatedAt = u.CreatedAt,
                    ProgressPercent = 0,
                    Status = (u.Status?.ToString() ?? "unknown").ToLower()
                }).ToList(),
                AllProjects = allProjects
            };

            return View(vm);
        }


       [HttpPost]
       [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMembers(AddMembersViewModel model)
        {
            if (model.ProjectId <= 0)
            {
                ModelState.AddModelError("", "Bạn phải chọn một dự án trước khi thêm thành viên.");
            }

            if (!ModelState.IsValid)
            {
                model.AllProjects = await _userProjectService.GetAllProjectsAsync();
                var users = await _userProjectService.GetAllUsersAsync();
                model.Users = users
                    .Select(u => new UserListItemVM
                    {
                        UserId = u.UserId,
                        FullName = u.FullName,
                        Email = u.Email,
                        AvatarUrl = u.AvatarUrl,
                        RoleName = u.Role?.RoleName ?? "",
                        CreatedAt = u.CreatedAt,
                        ProgressPercent = 0,
                        Status = (u.Status?.ToString() ?? "unknown").ToLower()
                    }).ToList();

                return View(model);
            }

            if (model.SelectedUserIds != null && model.SelectedUserIds.Any())
            {
                var result = await _userProjectService.AddUsersToProjectAsync(model.ProjectId, model.SelectedUserIds);

                TempData["AddResults"] = System.Text.Json.JsonSerializer.Serialize(result);

            }
            else
            {
                TempData["Info"] = "Không có thành viên nào được chọn.";
            }

            return RedirectToAction("AddMembers", new { id = model.ProjectId });
        }



    }
}
