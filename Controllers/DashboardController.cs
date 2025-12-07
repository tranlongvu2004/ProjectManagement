using Microsoft.AspNetCore.Mvc;
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
        public IActionResult Dashboard()
        {
            var tasks = _context.Tasks.ToList();
            int totalTasks = tasks.Count;
            int completedTasks = tasks.Count(t => t.Status == Models.TaskStatus.Completed);
            int stuckTasks = tasks.Count(t => t.Status == Models.TaskStatus.Stuck);
            int inProgressTasks = tasks.Count(t => t.Status == Models.TaskStatus.InProgress);

            ViewBag.Tasks = System.Text.Json.JsonSerializer.Serialize(tasks);
            ViewBag.TotalTasks = totalTasks;
            ViewBag.CompletedTasks = completedTasks;
            ViewBag.StuckTasks = stuckTasks;
            ViewBag.InProgressTasks = inProgressTasks;


            return View();
        }
    }
}
