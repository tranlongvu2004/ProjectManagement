using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PorjectManagement.Models;
using PorjectManagement.ViewModels;

namespace PorjectManagement.Controllers
{
    public class HistoryLogController : Controller
    {
        private readonly LabProjectManagementContext _context;

        public HistoryLogController(LabProjectManagementContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> HistoryLog()
        {
            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;

            var workedOn = await _context.ActivityLogs
                .Where(a => a.UserId == userId
                         && a.CreatedAt >= DateTime.Now.AddDays(-30))
                .OrderByDescending(a => a.CreatedAt)
                .Select(a => new ActivityLogViewModel
                {
                    ActivityLogId = a.ActivityLogId,
                    ProjectId = a.ProjectId,
                    TaskId = a.TaskId,
                    Message = a.Message,
                    CreatedAt = a.CreatedAt,
                    Project = a.Project
                })
                .ToListAsync();

            var assignedToMe = await _context.TaskAssignments
                .Where(t => t.UserId == userId)
                .Include(t => t.Task)
                .Select(t => new AssignedTaskViewModel
                {
                    TaskId = t.Task.TaskId,
                    ProjectId = t.Task.ProjectId,
                    TaskTitle = t.Task.Title,
                    ProjectName = t.Task.Project.ProjectName,
                    Deadline = t.Task.Deadline,
                    Status = t.Task.Status.ToString()
                })
                .ToListAsync();

            return View(new HistoryLogViewModel
            {
                WorkedOn = workedOn,
                AssignedToMe = assignedToMe
            });
        }
    }
}
