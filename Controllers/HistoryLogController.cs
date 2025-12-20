using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PorjectManagement.Models;

namespace PorjectManagement.Controllers
{
    public class HistoryLogController : Controller
    {
        private readonly LabProjectManagementContext _context;

        public HistoryLogController(LabProjectManagementContext context)
        {
            _context = context;
        }

        public IActionResult HistoryLog()
        {
            int? currentUserId = HttpContext.Session.GetInt32("UserId"); // lấy từ session

            var thirtyDaysAgo = DateTime.Now.AddDays(-30);

            ViewBag.WorkedOn = _context.ActivityLogs
                .Include(a => a.Project)
                .Include(a => a.Task)
                .Where(a => a.UserId == currentUserId &&
                            a.CreatedAt >= thirtyDaysAgo)
                .OrderByDescending(a => a.CreatedAt)
                .ToList();

            ViewBag.AssignedToMe = _context.TaskAssignments
                .Include(ta => ta.Task)
                    .ThenInclude(t => t.Project)
                .Where(ta => ta.UserId == currentUserId)
                .OrderByDescending(ta => ta.AssignedAt)
                .ToList();

            return View();
        }
    }
}
