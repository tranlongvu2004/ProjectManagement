using Microsoft.AspNetCore.Mvc;
using PorjectManagement.Models;

namespace PorjectManagement.Controllers
{
    public class TimelineController : Controller
    {
        protected LabProjectManagementContext _context;
        public TimelineController(LabProjectManagementContext context)
        {
            _context = context;
        }
        public IActionResult Timeline(int projectId)
        {
            var task = _context.Tasks
                .Where(t => t.ProjectId == projectId)
                .Select(t => new
                {
                    t.TaskId,
                    t.Title,
                    Start = t.CreatedAt,
                    End = t.Deadline,
                    Status = t.Status.ToString() ?? "Not_Started",
                    Owner = t.TaskAssignments.FirstOrDefault() != null ? t.TaskAssignments.FirstOrDefault()!.User.FullName : "Unassigned"
                })
                .ToList();
            ViewBag.Tasks = System.Text.Json.JsonSerializer.Serialize(task);
            ViewBag.ProjectId = projectId;

            return View();
        }
    }
}
