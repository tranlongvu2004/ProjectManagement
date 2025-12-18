using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PorjectManagement.Models;
using PorjectManagement.Service.Interface;
using PorjectManagement.ViewModels;

namespace PorjectManagement.Controllers
{
    public class TaskController : Controller
    {
        private readonly ITaskService _taskService;
        private readonly IUserProjectService _userProjectService;
        private readonly ICommentService _commentService;
        private readonly LabProjectManagementContext _context;

        public TaskController(
            ITaskService taskService,
            IUserProjectService userProjectService,
            ICommentService commentService,
            LabProjectManagementContext context)
        {
            _taskService = taskService;
            _userProjectService = userProjectService;
            _commentService = commentService;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> CreateTask(int? projectId)
        {
            int roleId = HttpContext.Session.GetInt32("RoleId") ?? 0;
            if (roleId != 2)
            {
                return RedirectToAction("AccessDeny", "Error");
            }
            var vm = new TaskCreateViewModel();
            if (!projectId.HasValue)
            {
                var referer = Request.Headers["Referer"].ToString();

                if (!string.IsNullOrEmpty(referer))
                {
                    var uri = new Uri(referer);
                    var query = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query);

                    if (query.TryGetValue("projectId", out var value))
                    {
                        projectId = int.Parse(value);
                    }
                }
            }
            if (!projectId.HasValue)
            {
                var projectList = await _userProjectService.GetAllProjectsAsync();
                ViewBag.ProjectList = new SelectList(projectList, "ProjectId", "ProjectName");
                vm.ProjectMembers = new List<PorjectManagement.Models.User>();
                return View(vm);
            }
            vm.ProjectId = projectId.Value;
            vm.ProjectMembers = await _userProjectService.GetUsersByProjectIdAsync(projectId.Value);
            ViewBag.HideProjectDropdown = true;
            var parentTasks = await _taskService.GetParentTasksByProjectAsync(projectId.Value);
            vm.ParentTasks = parentTasks.Select(t => new SelectListItem
            {
                Value = t.TaskId.ToString(),
                Text = t.Title
            }).ToList();
            return View(vm);
        }


        [HttpPost]
        public async Task<IActionResult> CreateTask(TaskCreateViewModel model)
        {
            int roleId = HttpContext.Session.GetInt32("RoleId") ?? 0;
            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;
            if (roleId != 2)
            {
                return RedirectToAction("AccessDeny", "Error");
            }
            var project = await _context.Projects
         .FirstOrDefaultAsync(p => p.ProjectId == model.ProjectId);

            if (project == null)
            {
                ModelState.AddModelError("", "Project does not exist");
            }
            else
            {
                if (model.Deadline.HasValue && model.Deadline.Value < DateTime.Now)
                {
                    ModelState.AddModelError(
                        "Deadline",
                        "Deadline cannot be in the past"
                    );
                }
                if (model.Deadline.HasValue && model.Deadline.Value > project.Deadline)
                {
                    ModelState.AddModelError(
                        "Deadline",
                        $"Task deadline cannot exceed project deadline ({project.Deadline:dd/MM/yyyy})"
                    );
                }
            }
            if (!ModelState.IsValid)
            {
                ViewBag.ProjectList = new SelectList(
                    await _userProjectService.GetAllProjectsAsync(),
                    "ProjectId",
                    "ProjectName"
                );
                model.ProjectMembers = await _userProjectService.GetUsersByProjectIdAsync(model.ProjectId);

                return View(model);
            }


            var task = new PorjectManagement.Models.Task
            {
                ProjectId = model.ProjectId,
                Title = model.Title,
                Description = model.Description,
                Priority = model.Priority,
                Status = PorjectManagement.Models.TaskStatus.ToDo,
                Deadline = model.Deadline,
                CreatedBy = userId,
                CreatedAt = DateTime.Now,
                IsParent = !model.IsSubTask,
                ParentId = model.IsSubTask ? model.ParentTaskId : null
            };

            var newTaskId = await _taskService.CreateTaskAsync(task);
            if (model.SelectedUserIds != null && model.SelectedUserIds.Any())
                await _taskService.AssignUsersToTaskAsync(newTaskId, model.SelectedUserIds);
            TempData["SuccessMessage"] = "Create Task successfully!";
            return RedirectToAction("BacklogUI", "Backlog", new { projectId = model.ProjectId });
        }
        public async Task<IActionResult> Assign(int id, bool success = false)
        {
            var vm = await _taskService.GetAssignTaskDataAsync(id);
            if (vm == null) return NotFound();

            ViewBag.Success = success;
            return View(vm);
        }


        [HttpPost]
        public async Task<IActionResult> Assign(TaskAssignViewModel model)
        {
            int roleId = HttpContext.Session.GetInt32("RoleId") ?? 0;
            if (roleId != 2)
            {
                return RedirectToAction("AccessDeny", "Error");
            }
            if (model.SelectedUserId <= 0)
            {
                ModelState.AddModelError("", "You must choose at least 1 member");
                model = await _taskService.GetAssignTaskDataAsync(model.TaskId);
                return View(model);
            }

            try
            {
                await _taskService.AssignTaskAsync(model.TaskId, model.SelectedUserId);
                ViewBag.SuccessMessage = "Assign task successfully!";
                return RedirectToAction("Assign", new { id = model.TaskId, success = true });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                model = await _taskService.GetAssignTaskDataAsync(model.TaskId);
                return View(model);
            }
        }

        // GET: /Task/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {

            int roleId = HttpContext.Session.GetInt32("RoleId") ?? 0;
            if (roleId != 2)
            {
                return RedirectToAction("AccessDeny", "Error");
            }
            var userEmail = HttpContext.Session.GetString("UserEmail");
            var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);

            if (currentUser == null)
            {
                TempData["Error"] = "Login information not found.";
                return RedirectToAction("Index", "Home");
            }

            var model = await _taskService.GetTaskForEditAsync(id, currentUser.UserId);

            if (model == null)
            {
                TempData["Error"] = "Task not found, or you do not have editing permissions.";
                return RedirectToAction("Index", "Home");
            }

            // Lấy project deadline vào view
            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.ProjectId == model.ProjectId);

            if (project != null)
            {
                ViewBag.ProjectDeadline = project.Deadline;
            }

            return View(model);
        }

        // POST: /Task/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TaskEditViewModel model)
        {
            int roleId = HttpContext.Session.GetInt32("RoleId") ?? 0;
            if (roleId != 2)
            {
                return RedirectToAction("AccessDeny", "Error");
            }
            if (id != model.TaskId)
            {
                TempData["Error"] = "Data not valid.";
                return RedirectToAction("Index", "Home");
            }

            // Lấy project validate deadline
            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.ProjectId == model.ProjectId);

            if (project == null)
            {
                ModelState.AddModelError("", "Project does not exist");
            }
            else
            {
                if (model.Deadline.HasValue && model.Deadline.Value < DateTime.Now)
                {
                    ModelState.AddModelError("Deadline", "Deadline not in past");
                }

                // Validate: Task deadline < project deadline
                if (model.Deadline.HasValue && model.Deadline.Value > project.Deadline)
                {
                    ModelState.AddModelError(
                        "Deadline",
                        $"Task deadline cannot exceed project deadline ({project.Deadline:dd/MM/yyyy})"
                    );
                }
            }

            // Validate: No assign task Mentor
            if (model.SelectedUserIds != null && model.SelectedUserIds.Any())
            {
                var mentorIds = await _context.Users
                    .Where(u => model.SelectedUserIds.Contains(u.UserId) && u.RoleId == 1)
                    .Select(u => u.UserId)
                    .ToListAsync();

                if (mentorIds.Any())
                {
                    ModelState.AddModelError("SelectedUserIds", "Cannot assign task to Mentor.");
                }
            }

            if (!ModelState.IsValid)
            {
                // Reload data
                var reloadedModel = await _taskService.GetTaskForEditAsync(id, 1);
                if (reloadedModel != null)
                {
                    model.ProjectMembers = reloadedModel.ProjectMembers;
                    model.CurrentAssignees = reloadedModel.CurrentAssignees;
                }

                // Truyền lại ProjectDeadline cho view
                if (project != null)
                {
                    ViewBag.ProjectDeadline = project.Deadline;
                }

                return View(model);
            }

            try
            {
                var userEmail = HttpContext.Session.GetString("UserEmail");
                var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);

                bool success = await _taskService.UpdateTaskAsync(model, currentUser.UserId);

                if (!success)
                {
                    TempData["Error"] = "Cant update task.";
                    return RedirectToAction("BacklogUI", "Backlog", new { projectId = model.ProjectId });
                }

                TempData["Success"] = "Update task successful!";
                return RedirectToAction("BacklogUI", "Backlog", new { projectId = model.ProjectId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error: {ex.Message}");
                var reloadedModel = await _taskService.GetTaskForEditAsync(id, 1);
                if (reloadedModel != null)
                {
                    model.ProjectMembers = reloadedModel.ProjectMembers;
                    model.CurrentAssignees = reloadedModel.CurrentAssignees;
                }

                // Truyền lại ProjectDeadline cho view
                if (project != null)
                {
                    ViewBag.ProjectDeadline = project.Deadline;
                }

                return View(model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> UploadAttachment(int taskId, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return RedirectToAction("BacklogUI", new
                {
                    projectId = _context.Tasks
        .Where(t => t.TaskId == taskId)
        .Select(t => t.ProjectId)
        .First()
                });

            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return Unauthorized();

            // 📁 tạo thư mục
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid()}_{file.FileName}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var attachment = new TaskAttachment
            {
                TaskId = taskId,
                FileName = Path.GetFileName(file.FileName),
                FilePath = "/uploads/" + fileName,
                FileType = file.ContentType,
                FileSize = file.Length,
                UploadedBy = userId.Value,
                UploadedAt = DateTime.Now
            };

            _context.TaskAttachments.Add(attachment);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Upload file successfully!";
            return RedirectToAction("BacklogUI", "Backlog", new
            {
                projectId = _context.Tasks
        .Where(t => t.TaskId == taskId)
        .Select(t => t.ProjectId)
        .First()
            });
        }

        // POST: /Task/AddComment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(int taskId, string content)
        {
            int roleId = HttpContext.Session.GetInt32("RoleId") ?? 0;
            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;

            // Chỉ Intern/InternLead được comment
            if (roleId != 2 || userId == 0)
            {
                TempData["Error"] = "You don't have permission to add comment.";
                return RedirectToAction("BacklogUI", "Backlog");
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                TempData["Error"] = "Comment cannot be empty.";
                return RedirectToAction("BacklogUI", "Backlog", new { projectId = ViewBag.ProjectId });
            }

            var success = await _commentService.AddCommentAsync(taskId, userId, content);

            if (success)
            {
                TempData["Success"] = "Comment added successfully!";
            }
            else
            {
                TempData["Error"] = "Failed to add comment.";
            }

            // Quay lại trang BacklogUI
            var projectId = await _context.Tasks
                .Where(t => t.TaskId == taskId)
                .Select(t => t.ProjectId)
                .FirstOrDefaultAsync();

            return RedirectToAction("BacklogUI", "Backlog", new { projectId = projectId });
        }

        // GET: /api/comments/{taskId}
        [HttpGet("/api/comments/{taskId}")]
        public async Task<IActionResult> GetComments(int taskId)
        {
            var comments = await _commentService.GetCommentsByTaskIdAsync(taskId);
            return Json(comments);
        }
    }
}
