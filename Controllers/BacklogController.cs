using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PorjectManagement.Models;
using PorjectManagement.Service.Interface;

namespace PorjectManagement.Controllers
{
    public class BacklogController : BaseController
    {
        private readonly LabProjectManagementContext _context;
        private readonly IProjectServices _projectServices;

        public BacklogController(
            LabProjectManagementContext context,
            IProjectServices projectServices)
        {
            _context = context;
            _projectServices = projectServices;
        }
        public IActionResult BacklogUI(int projectId)
        {
            var redirect = RedirectIfNotLoggedIn();
            if (redirect != null) return redirect;

            // Theo dev/Vu
            var tasks = _context.Tasks
                .Include(t => t.TaskAssignments)
                .ThenInclude(ta => ta.User)
                .Include(t => t.TaskAttachments)
                .Include(t => t.Comments)
                .Where(t => t.ProjectId == projectId)
                .ToList();
            var parentTasks = tasks.Where(t => t.ParentId == null && t.IsParent == true).ToList();
            var subTasks = tasks.Where(t => t.ParentId != null && t.IsParent == false).ToList();

            ViewBag.ParentTasks = parentTasks;
            ViewBag.SubTasks = subTasks;
            ViewBag.ProjectId = projectId;

            // ViewBag.Projects tự động load từ BaseController.OnActionExecuting

            return View();
        }

        [HttpPost]
        public IActionResult DeleteTask([FromBody] DeleteTaskRequest request)
        {
            var task = _context.Tasks
                .Include(t => t.TaskAssignments)
                .ThenInclude(ta => ta.User)
                .Include(t => t.TaskAttachments)
                .Include(t => t.Comments)
                .FirstOrDefault(t => t.TaskId == request.TaskId);

            if (task == null) return NotFound();
            bool hasChildren = _context.Tasks.Any(t => t.ParentId == task.TaskId);
            if (hasChildren)
                return BadRequest("Task has subtasks");

            var snapshot = new DTOTaskSnapshot
            {
                TaskId = task.TaskId,
                TaskName = task.Title,
                CreatedAt = task.CreatedAt,
                Deadline = task.Deadline,
                Owner = task.TaskAssignments.FirstOrDefault()?.User.FullName ?? "Unassigned",
                Status = task.Status.ToString(),
                ProjectId = task.ProjectId
            };

            var recycle = new RecycleBin
            {
                EntityType = "Task",
                EntityId = task.TaskId,
                DataSnapshot = System.Text.Json.JsonSerializer.Serialize(snapshot),
                DeletedBy = HttpContext.Session.GetInt32("UserId") ?? 0,
                DeletedAt = DateTime.Now
            };

            _context.RecycleBins.Add(recycle);
            _context.TaskAssignments.RemoveRange(task.TaskAssignments);
            _context.Comments.RemoveRange(task.Comments);
            _context.TaskAttachments.RemoveRange(task.TaskAttachments);

            _context.Tasks.Remove(task);

            _context.SaveChanges();

            return Ok();
        }
    }

    public class DeleteTaskRequest
    {
        public int TaskId { get; set; }
    }

}