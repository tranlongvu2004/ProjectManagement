using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using PorjectManagement.Models;

namespace PorjectManagement.Controllers
{
    public class DashboardController : Controller
    {
        protected LabProjectManagementContext _context;
        public DashboardController(LabProjectManagementContext context)
        {
            _context = context;
        }
        public IActionResult Dashboard(int projectId, int userId)
        {
            var tasks = _context.Tasks
                .Select(t => new
                {
                    t.ProjectId,
                    t.Title,
                    Status = t.Status.ToString() ?? "Not_Started",
                    Owner = t.CreatedByNavigation.FullName ?? "Unknown"
                })
                .Where(t => t.ProjectId == projectId)
                .ToList();

            var ownerTasks = _context.Tasks
                .Where(t => t.ProjectId == projectId && t.TaskAssignments.Any(ta => ta.UserId == userId))
                .Select(t => new
                {
                    t.TaskId,
                    t.Title,
                    Status = t.Status.ToString() ?? "Not_Started"
                })
                .ToList();

            int totalTasks = tasks.Count;
            int completedTasks = tasks.Count(t => t.Status == "Completed");
            int stuckTasks = tasks.Count(t => t.Status == "Stuck");
            int inProgressTasks = tasks.Count(t => t.Status == "Doing");

            ViewBag.OwnerTasks = System.Text.Json.JsonSerializer.Serialize(ownerTasks);
            ViewBag.Tasks = System.Text.Json.JsonSerializer.Serialize(tasks);   
            ViewBag.TotalTasks = totalTasks;
            ViewBag.CompletedTasks = completedTasks;
            ViewBag.StuckTasks = stuckTasks;
            ViewBag.InProgressTasks = inProgressTasks;
            ViewBag.projectId = projectId;
            ViewBag.UserId = HttpContext.Session.GetInt32("UserId") ?? 0;


            return View();
        }
        [HttpGet]
        public IActionResult GetTasks(int projectId)
        {
            var tasks = _context.Tasks
                .Select(t => new
                {
                    t.ProjectId,
                    t.Title,
                    Status = t.Status.ToString(),
                    Owner = t.CreatedByNavigation.FullName ?? "Unknown"
                })
                .Where(t => t.ProjectId == projectId)
                .ToList();

            return Json(tasks);
        }

        [HttpGet]
        public IActionResult GetTasksByUserId(int projectId, int userId)
        {
            var tasks = _context.TaskAssignments
                .Where(ta => ta.Task.ProjectId == projectId && ta.UserId == userId)
                .Include(ta => ta.Task)
                .Select(a => new {
                    a.Task.Title,
                    Status = a.Task.Status.ToString()
                })
                .ToList();

            return Json(tasks);
        }

        public IActionResult DashboardPartial(int projectId, int userId)
        {
            return View("Dashboard"); // trả về cùng model Dashboard luôn
        }

    }
}
