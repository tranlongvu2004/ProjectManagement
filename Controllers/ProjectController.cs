using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PorjectManagement.Models;
using PorjectManagement.Service.Interface;
using PorjectManagement.ViewModels;

namespace PorjectManagement.Controllers
{
    public class ProjectController : Controller
    {
        private readonly IProjectServices _projectServices;

        public ProjectController(IProjectServices projectServices)
        {
            _projectServices = projectServices;
        }
        public async Task<IActionResult> Index(
            string? keyword,
            ProjectStatus? status,
            string sortBy = "name",
            string sortDir = "asc",
            int page = 1)
        {
            int pageSize = 7;
            int currentUserId = HttpContext.Session.GetInt32("UserId") ?? 0;
            int roleId = HttpContext.Session.GetInt32("RoleId") ?? 0;
            if (currentUserId == 0)
            {
                return RedirectToAction("Login", "User");
            }
            ViewBag.RoleId = roleId;
            var projects = await _projectServices.GetProjectsOfUserAsync(currentUserId);
            
            var sidebarVB = projects
                .Select(p => new ProjectListVM
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

            var query = projects
                .Select(p => new ProjectListVM
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
                });

            if (!string.IsNullOrEmpty(keyword))
                query = query.Where(x => x.ProjectName.ToLower().Contains(keyword.ToLower()));

            if (status.HasValue)
                query = query.Where(x => x.Status == status);

            query = (sortBy, sortDir) switch
            {
                ("deadline", "asc") => query.OrderBy(x => x.Deadline),
                ("deadline", "desc") => query.OrderByDescending(x => x.Deadline),
                ("name", "desc") => query.OrderByDescending(x => x.ProjectName),
                _ => query.OrderBy(x => x.ProjectName)
            };

            int totalItems = query.Count();

            var sortedProjects = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select((p, i) => new ProjectListVM
                {
                    Index = (page - 1) * pageSize + i + 1,
                    ProjectId = p.ProjectId,
                    ProjectName = p.ProjectName,
                    LeaderName = p.LeaderName,
                    MemberCount = p.MemberCount,
                    Deadline = p.Deadline,
                    Status = p.Status
                })
                .ToList();

            var vm = new ProjectFilterVM
            {
                Keyword = keyword,
                Status = status,
                SortBy = sortBy,
                SortDir = sortDir,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
                Projects = sortedProjects
            };
            ViewBag.Projects = sidebarVB;
            return View(vm);
        }

    }
}
