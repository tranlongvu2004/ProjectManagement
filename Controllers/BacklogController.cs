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
        private readonly ICommentService _commentService;

        public BacklogController(
            LabProjectManagementContext context,
            IProjectServices projectServices,
            IUserProjectService up,
            ICommentService commentService)
        {
            _context = context;
            _projectServices = projectServices;
            _up = up;
            _commentService = commentService;
        }

        public async Task<IActionResult> BacklogUI(int projectId)
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

            var tasks = await _context.Tasks
                .Include(t => t.TaskAssignments)
                    .ThenInclude(ta => ta.User)
                .Include(t => t.TaskAttachments)
                    .ThenInclude(a => a.UploadedByNavigation)
                .Include(t => t.Comments)
                    .ThenInclude(c => c.User)
                        .ThenInclude(u => u.Role)
                .Where(t => t.ProjectId == projectId
                    && !_context.RecycleBins.Any(r =>
                        r.EntityType == "Task"
                        && r.EntityId == t.TaskId)
                    && !deletedTasks.Contains(t.TaskId))
                .ToListAsync();

            var parentTasks = tasks.Where(t => t.ParentId == null && t.IsParent == true).ToList();
            var subTasks = tasks.Where(t => t.ParentId != null && t.IsParent == false).ToList();

            ViewBag.ParentTasks = parentTasks;
            ViewBag.SubTasks = subTasks;
            ViewBag.ProjectId = projectId;
            ViewBag.IsLeader = isLeader;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> DeleteTask([FromBody] DeleteTaskRequest request)
        {
            int roleId = HttpContext.Session.GetInt32("RoleId") ?? 0;
            if (roleId != 2)
            {
                return RedirectToAction("AccessDeny", "Error", new { returnUrl = HttpContext.Request.Path + HttpContext.Request.QueryString });
            }

            var task = await _context.Tasks
                .Include(t => t.TaskAssignments)
                    .ThenInclude(ta => ta.User)
                .Include(t => t.TaskAttachments)
                .Include(t => t.Comments)
                .FirstOrDefaultAsync(t => t.TaskId == request.TaskId);

            if (task == null) return NotFound();

            // Completed task cannot be deleted
            if (task.Status == Models.TaskStatus.Completed)
            {
                return BadRequest("Completed task cannot be deleted.");
            }

            // Lưu projectId trước khi delete 
            int projectId = task.ProjectId;

            int deletedBy = HttpContext.Session.GetInt32("UserId") ?? 0;

            // Delete task recursively (dev/duc)
            DeleteTaskRecursive(task, deletedBy);

            await _context.SaveChangesAsync();

            // NEW: Update project status sau khi delete task
            await _projectServices.UpdateProjectStatusAsync(projectId);

            return Ok();
        }

        // Xóa task và tất cả subtasks (recursive)
        private void DeleteTaskRecursive(Models.Task task, int deletedBy)
        {
            // Validate Completed
            if (task.Status == Models.TaskStatus.Completed)
                throw new Exception("Cannot delete completed task");

            // Delete subtask first
            var children = _context.Tasks
                .Include(t => t.TaskAssignments).ThenInclude(ta => ta.User)
                .Include(t => t.TaskAttachments)
                .Include(t => t.Comments)
                .Where(t => t.ParentId == task.TaskId)
                .ToList();

            foreach (var child in children)
            {
                DeleteTaskRecursive(child, deletedBy);
            }

            // Snapshot
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

            _context.RecycleBins.Add(new RecycleBin
            {
                EntityType = "Task",
                EntityId = task.TaskId,
                DataSnapshot = System.Text.Json.JsonSerializer.Serialize(snapshot),
                DeletedBy = deletedBy,
                DeletedAt = DateTime.Now
            });
        }
    }

    public class DeleteTaskRequest
    {
        public int TaskId { get; set; }
    }
}