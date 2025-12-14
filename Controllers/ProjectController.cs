using Microsoft.AspNetCore.Mvc;
using PorjectManagement.Models;
using PorjectManagement.Service.Interface;
using PorjectManagement.ViewModels;

namespace PorjectManagement.Controllers
{
    public class ProjectController : BaseController
    {
        private readonly IProjectServices _projectServices;

        public ProjectController(IProjectServices projectServices)
        {
            _projectServices = projectServices;
        }

        // GET: /Project
        public async Task<IActionResult> Index()
        {
            var redirect = RedirectIfNotLoggedIn();
            if (redirect != null) return redirect;

            int currentUserId = HttpContext.Session.GetInt32("UserId") ?? 0;
            int roleId = HttpContext.Session.GetInt32("RoleId") ?? 0;
            if (currentUserId == 0)
            {
                return RedirectToAction("Login", "User");
            }
            ViewBag.RoleId = roleId;
            var projects = await _projectServices.GetProjectsOfUserAsync(currentUserId);

            var model = projects.Select(p => new ProjectListVM
            {
                ProjectId = p.ProjectId,
                ProjectName = p.ProjectName,
                Deadline = p.Deadline,
                Status = p.Status,
                LeaderName = p.UserProjects
                    .Where(x => x.IsLeader == true)
                    .Select(x => x.User.FullName)
                    .FirstOrDefault() ?? "Không xác định",
                MemberCount = p.UserProjects.Count
            }).ToList();

            ViewBag.Projects = model;
            return View(model);
        }
    }
}
