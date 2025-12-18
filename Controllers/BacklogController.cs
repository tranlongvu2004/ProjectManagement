using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PorjectManagement.Models;
using PorjectManagement.Service.Interface;

namespace PorjectManagement.Controllers
{
    public class BacklogController : Controller
    {
        private readonly LabProjectManagementContext _context;
        private readonly IProjectServices _projectServices;
        private readonly IUserProjectService _up;

        public BacklogController(
            LabProjectManagementContext context,
            IProjectServices projectServices,
            IUserProjectService up)
        {
            _context = context;
            _projectServices = projectServices;
            _up = up;
        }
        public IActionResult BacklogUI(int projectId)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "User");
            }
            bool isLeader = _up.IsleaderOfProject(userId.Value, projectId);
            // Theo dev/Vu
            var deletedTasks = _context.RecycleBins
                .Where(rb => rb.EntityType == "Task")
                .Select(rb => rb.EntityId).ToHashSet();

            var tasks = _context.Tasks
                .Include(t => t.TaskAssignments)
                  .ThenInclude(ta => ta.User)
                .Include(t => t.TaskAttachments)
                  .ThenInclude(a => a.UploadedByNavigation)
            .Where(t => t.ProjectId == projectId
                && !_context.RecycleBins.Any(r =>
                r.EntityType == "Task"
                && r.EntityId == t.TaskId) && !deletedTasks.Contains(t.TaskId))
            .ToList();
            var parentTasks = tasks.Where(t => t.ParentId == null && t.IsParent == true).ToList();
            var subTasks = tasks.Where(t => t.ParentId != null  && t.IsParent == false).ToList();

            ViewBag.ParentTasks = parentTasks;
            ViewBag.SubTasks = subTasks;
            ViewBag.ProjectId = projectId;
            ViewBag.IsLeader = isLeader;
            return View();
        }

        [HttpPost]
        public IActionResult DeleteTask([FromBody] DeleteTaskRequest request)
        {
            int roleId = HttpContext.Session.GetInt32("RoleId") ?? 0;
            if (roleId != 2)
            {
                return RedirectToAction("AccessDeny", "Error");
            }
            var task = _context.Tasks
                .Include(t => t.TaskAssignments)
                .ThenInclude(ta => ta.User)
                .Include(t => t.TaskAttachments)
                .Include(t => t.Comments)
                .FirstOrDefault(t => t.TaskId == request.TaskId);
            if (task == null) return NotFound();            
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
            _context.SaveChanges();
            return Ok();
        }
    }

    public class DeleteTaskRequest
    {
        public int TaskId { get; set; }
    }
}