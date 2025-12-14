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
        public IActionResult RecycleBin()
        {
            var data = _context.RecycleBins
                .Where(rb => rb.EntityType == "Task")
                .OrderByDescending(rb => rb.DeletedAt)
                .Select(x => new
            {
                Recycle = x,
                DeletedUser = _context.Users.FirstOrDefault(u => u.UserId == x.DeletedBy)
            })
            .AsEnumerable()
            .Select(x =>
        {
        var task = System.Text.Json.JsonSerializer.Deserialize<DTOTaskSnapshot>(x.Recycle.DataSnapshot);

        return new RecyclebinVM
        {
            RecycleId = x.Recycle.RecycleId,
            EntityType = "Task",
            Name = task?.TaskName ?? "(Unknown Task)",  
            Owner = task?.Owner ?? "Unassigned",
            Status = task?.Status ?? "Unknown",
            DeletedBy = x.DeletedUser?.FullName ?? "Unknown",
            DeletedAt = x.Recycle.DeletedAt ?? DateTime.MinValue,
            ProjectId = task?.ProjectId ?? 0
        };
        })
        .ToList();

            return View(data);
        }

        [HttpPost]
        public IActionResult DeletePermanent([FromBody] RestoreRequest request)
        {
            var item = _context.RecycleBins.FirstOrDefault(x => x.RecycleId == request.RecycleId);
            var task = _context.Tasks
                .Include(t => t.TaskAssignments)
                .Include(t => t.Comments)
                .Include(t => t.TaskAttachments)
                .FirstOrDefault(t => t.TaskId == item.EntityId);

            if(task == null) return NotFound();
            if (item == null) return NotFound();

            _context.TaskAssignments.RemoveRange(task.TaskAssignments);
            _context.Comments.RemoveRange(task.Comments);
            _context.TaskAttachments.RemoveRange(task.TaskAttachments);

            _context.Tasks.Remove(task);
            _context.RecycleBins.Remove(item);
            _context.SaveChanges();
            return Ok();
        }

        [HttpPost]
        public IActionResult Restore([FromBody] RestoreRequest request)
        {
            var item = _context.RecycleBins.FirstOrDefault(x => x.RecycleId == request.RecycleId);
            if (item == null) return NotFound();
            _context.RecycleBins.Remove(item);
            _context.SaveChanges();
            return Ok();
        }

        
    }
    public class RestoreRequest
        {
            public int RecycleId { get; set; }
        }
}
