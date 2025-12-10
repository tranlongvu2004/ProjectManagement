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
        public IActionResult Dashboard(int projectId)
        {
            var tasks = _context.Tasks
                .Select(t => new
                {
                    t.ProjectId,
                    t.Title,
                    Status = t.Status.ToString()
                })
                .Where(t => t.ProjectId == projectId)
                .ToList();
            int totalTasks = tasks.Count;
            int completedTasks = tasks.Count(t => t.Status == "Completed");
            int stuckTasks = tasks.Count(t => t.Status == "Stuck");
            int inProgressTasks = tasks.Count(t => t.Status == "Doing");

            ViewBag.Tasks = System.Text.Json.JsonSerializer.Serialize(tasks);   
            ViewBag.TotalTasks = totalTasks;
            ViewBag.CompletedTasks = completedTasks;
            ViewBag.StuckTasks = stuckTasks;
            ViewBag.InProgressTasks = inProgressTasks;


            return View();
        }
    }
}
