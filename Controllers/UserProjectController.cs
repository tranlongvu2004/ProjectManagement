using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PorjectManagement.Service.Interface;
using PorjectManagement.ViewModels;
using System.Linq;
using System.Threading.Tasks;

namespace PorjectManagement.Controllers
{
    public class UserProjectController : BaseController
    {
        private readonly IUserProjectService _userProjectService;

        public UserProjectController(IUserProjectService userProjectService)
        {
            _userProjectService = userProjectService;
        }
        [HttpGet]
        public async Task<IActionResult> AddMembers(int projectId)
        {
            var role = HttpContext.Session.GetInt32("RoleId");
            if (role != 2)
            {
                TempData["Error"] = "You are not allowed to do this action.";
                return RedirectToAction("Index", "Home");
            }

            var redirect = RedirectIfNotLoggedIn();
            if (redirect != null) return redirect;

            var project = await _userProjectService.GetProjectByIdAsync(projectId);
            if (project == null) return NotFound();

            var usersInProject = await _userProjectService.GetUsersByProjectIdAsync(projectId);
            var userIdsInProject = usersInProject.Select(u => u.UserId).ToHashSet();

            var allUsers = await _userProjectService.GetAllUsersAsync();

            var availableUsers = allUsers
                .Where(u => !userIdsInProject.Contains(u.UserId))
                .ToList();

            var vm = new AddMembersViewModel
            {
                ProjectId = project.ProjectId,
                ProjectName = project.ProjectName,
                Users = availableUsers.Select(u => new UserListItemVM
                {
                    UserId = u.UserId,
                    FullName = u.FullName,
                    Email = u.Email,
                    AvatarUrl = u.AvatarUrl,
                    RoleName = u.Role?.RoleName ?? "",
                    CreatedAt = u.CreatedAt,
                    Status = u.Status.ToString().ToLower()
                }).ToList()
            };

            return View(vm);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMembers(AddMembersViewModel model)
        {
            var role = HttpContext.Session.GetInt32("RoleId");
            if (role != 2)
            {
                TempData["Error"] = "You are not allowed to do this action.";
                return RedirectToAction("Index", "Home");
            }

            var redirect = RedirectIfNotLoggedIn();
            if (redirect != null) return redirect;

            if (model.ProjectId <= 0)
            {
                ModelState.AddModelError("", "Invalid project.");
            }

            if (!ModelState.IsValid)
            {
                var usersInProject = await _userProjectService.GetUsersByProjectIdAsync(model.ProjectId);
                var userIdsInProject = usersInProject.Select(u => u.UserId).ToHashSet();

                var allUsers = await _userProjectService.GetAllUsersAsync();
                model.Users = allUsers
                    .Where(u => !userIdsInProject.Contains(u.UserId))
                    .Select(u => new UserListItemVM
                    {
                        UserId = u.UserId,
                        FullName = u.FullName,
                        Email = u.Email,
                        AvatarUrl = u.AvatarUrl,
                        RoleName = u.Role?.RoleName ?? "",
                        CreatedAt = u.CreatedAt,
                        Status = u.Status.ToString().ToLower()
                    }).ToList();

                return View(model);
            }

            if (model.SelectedUserIds != null && model.SelectedUserIds.Any())
            {
                var result = await _userProjectService
                    .AddUsersToProjectAsync(model.ProjectId, model.SelectedUserIds);

                TempData["AddResults"] =
                    System.Text.Json.JsonSerializer.Serialize(result);
            }
            else
            {
                TempData["Info"] = "No member is selected.";
            }

            return RedirectToAction("AddMembers", new { projectId = model.ProjectId });
        }

    }
}