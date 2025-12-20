using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PorjectManagement.Models;
using PorjectManagement.ViewModels;

namespace PorjectManagement.Controllers
{
    public class RecycleBinController : Controller
    {
        private readonly LabProjectManagementContext _context;
        public RecycleBinController(LabProjectManagementContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> RecycleBin()
        {
            var recycleBins = await _context.RecycleBins
                    .Where(rb => rb.EntityType == "Task")
                    .OrderByDescending(rb => rb.DeletedAt)
                    .ToListAsync();

            var userIds = recycleBins
                .Select(x => x.DeletedBy)
                .Distinct()
                .ToList();

            var users = await _context.Users
                .Where(u => userIds.Contains(u.UserId))
                .ToDictionaryAsync(u => u.UserId);

            var data = recycleBins.Select(x =>
            {
                var task = System.Text.Json.JsonSerializer
                    .Deserialize<DTOTaskSnapshot>(x.DataSnapshot);

                users.TryGetValue(x.DeletedBy ?? 0, out var deletedUser);

                return new RecyclebinVM
                {
                    RecycleId = x.RecycleId,
                    EntityType = "Task",
                    Name = task?.TaskName ?? "(Unknown Task)",
                    Owner = task?.Owner ?? "Unassigned",
                    Status = task?.Status ?? "Unknown",
                    DeletedBy = deletedUser?.FullName ?? "Unknown",
                    DeletedAt = x.DeletedAt ?? DateTime.MinValue,
                    ProjectId = task?.ProjectId ?? 0
                };
            }).ToList();


            return View(data);
        }

        [HttpPost]
        public async Task<IActionResult> DeletePermanent([FromBody] RestoreRequest request)
        {
            int roleId = HttpContext.Session.GetInt32("RoleId") ?? 0;
            if (roleId != 2)
            {
                return RedirectToAction("AccessDeny", "Error", new { returnUrl = HttpContext.Request.Path + HttpContext.Request.QueryString });
            }
            var item = await _context.RecycleBins.FirstOrDefaultAsync(x => x.RecycleId == request.RecycleId);
            if (item == null) return NotFound();


            var task = await _context.Tasks
                .Include(t => t.TaskAssignments)
                .Include(t => t.Comments)
                .Include(t => t.TaskAttachments)
                .Include(t => t.ActivityLogs)
                .Include(t => t.TaskHistories)
                .FirstOrDefaultAsync(t => t.TaskId == item.EntityId);

            if (task == null) return NotFound();


            _context.TaskAssignments.RemoveRange(task.TaskAssignments);
            _context.Comments.RemoveRange(task.Comments);
            _context.TaskAttachments.RemoveRange(task.TaskAttachments);
            _context.ActivityLogs.RemoveRange(task.ActivityLogs);
            _context.TaskHistories.RemoveRange(task.TaskHistories);

            _context.Tasks.Remove(task);
            _context.RecycleBins.Remove(item);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> Restore([FromBody] RestoreRequest request)
        {
            int roleId = HttpContext.Session.GetInt32("RoleId") ?? 0;
            if (roleId != 2)
            {
                return RedirectToAction("AccessDeny", "Error", new { returnUrl = HttpContext.Request.Path });
            }
            var item = await _context.RecycleBins.FirstOrDefaultAsync(x => x.RecycleId == request.RecycleId);
            if (item == null) return NotFound();

            var task = await _context.Tasks.FirstOrDefaultAsync(t => t.TaskId == item.EntityId);
            if (task == null) return NotFound();

            if (task.ParentId != null)
            {
                bool parentStillDeleted = await _context.RecycleBins.AnyAsync(rb =>
                    rb.EntityType == "Task" &&
                    rb.EntityId == task.ParentId);

                if (parentStillDeleted)
                {
                    return BadRequest("You must restore parent task first.");
                }
            }
            ;

            _context.RecycleBins.Remove(item);
            await _context.SaveChangesAsync();
            return Ok();
        }


    }
    public class RestoreRequest
    {
        public int RecycleId { get; set; }
    }
}
