using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        public async Task<IActionResult> Timeline(int projectId)
        {
            var task = await _context.Tasks
                .Where(t => t.ProjectId == projectId)
                .Select(t => new
                {
                    t.TaskId,
                    t.Title,
                    Start = t.CreatedAt.HasValue
                    ? t.CreatedAt.Value.Date
                    : (DateTime?)null,
                    End = t.Deadline.HasValue
                    ? t.Deadline.Value.Date.AddDays(1)
                    : (DateTime?)null,
                    Status = t.Status.ToString() ?? "Not_Started",
                    Owner = t.TaskAssignments.FirstOrDefault() != null ? t.TaskAssignments.FirstOrDefault()!.User.FullName : "Unassigned"
                })
                .ToListAsync();
            ViewBag.Tasks = System.Text.Json.JsonSerializer.Serialize(task);
            ViewBag.ProjectId = projectId;

            return View();
        }
    }
}
