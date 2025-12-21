using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Build.Evaluation;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using PorjectManagement.Models;
using PorjectManagement.Service;
using PorjectManagement.Service.Interface;
using PorjectManagement.ViewModels;
using System.ComponentModel.Design;

namespace PorjectManagement.Controllers
{
    public class TaskController : Controller
    {
        private readonly ITaskService _taskService;
        private readonly IUserProjectService _userProjectService;
        private readonly ICommentService _commentService;
        private readonly IProjectServices _projectServices;
        private readonly LabProjectManagementContext _context;
        private readonly IActivityLogService _activityLogService;
        private readonly ITaskHistoryService _taskHistoryService;

        public TaskController(
            ITaskService taskService,
            IUserProjectService userProjectService,
            ICommentService commentService,
            IProjectServices projectServices,
            LabProjectManagementContext context,
            IActivityLogService activityLogService,
            ITaskHistoryService taskHistoryService)
        {
            _taskService = taskService;
            _userProjectService = userProjectService;
            _commentService = commentService;
            _projectServices = projectServices;
            _context = context;
            _activityLogService = activityLogService;
            _taskHistoryService = taskHistoryService;
        }

        [HttpGet]
        public async Task<IActionResult> CreateTask(int? projectId)
        {
            int roleId = HttpContext.Session.GetInt32("RoleId") ?? 0;
            if (roleId != 2)
            {
                return RedirectToAction("AccessDeny", "Error", new { returnUrl = HttpContext.Request.Path + HttpContext.Request.QueryString });
            }
            var vm = new TaskCreateViewModel();
            if (!projectId.HasValue)
            {
                var referer = Request.Headers["Referer"].ToString();

                if (!string.IsNullOrEmpty(referer))
                {
                    // Thử lấy projectId từ URL trước đó (Referer)
                    var uri = new Uri(referer);
                    // Parse query string để tìm projectId
                    var query = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query);

                    if (query.TryGetValue("projectId", out var value))
                    {
                        projectId = int.Parse(value);
                    }
                }
            }
            if (!projectId.HasValue)
            {
                // Lấy danh sách tất cả project để người dùng chọn
                var projectList = await _userProjectService.GetAllProjectsAsync();
                ViewBag.ProjectList = new SelectList(projectList, "ProjectId", "ProjectName");
                vm.ProjectMembers = new List<PorjectManagement.Models.User>();
                return View(vm);
            }
            vm.ProjectId = projectId.Value;
            vm.ProjectMembers = await _userProjectService.GetUsersByProjectIdNoMentorAsync(projectId.Value);
            // Ẩn dropdown chọn project vì project đã được xác định
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
                return RedirectToAction("AccessDeny", "Error", new { returnUrl = HttpContext.Request.Path });
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
                model.ProjectMembers = await _userProjectService.GetUsersByProjectIdNoMentorAsync(model.ProjectId);

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

            //Duc Nghiem
            _activityLogService.Log(

                userId: userId,
                projectId: model.ProjectId,
                taskId: newTaskId,
                actionType: "TASK_CREATED",
                message: $"created task \"{task.Title}\"",
                createdAt: DateTime.Now
            );
            await _taskHistoryService.AddAsync(
             newTaskId,
                userId,
             "TASK_CREATED",
             $"create task \"{task.Title}\""
 );



            if (model.SelectedUserIds != null && model.SelectedUserIds.Any())
                await _taskService.AssignUsersToTaskAsync(newTaskId, model.SelectedUserIds);

            await _projectServices.UpdateProjectStatusAsync(model.ProjectId);
            await _context.SaveChangesAsync();

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
                return RedirectToAction("AccessDeny", "Error", new { returnUrl = HttpContext.Request.Path });
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

                //Duc Nghiem
                var task = await _context.Tasks
                    .Include(t => t.Project)
                    .FirstAsync(t => t.TaskId == model.TaskId);

                var assignedUser = await _context.Users
                    .FirstAsync(u => u.UserId == model.SelectedUserId);

                var currentUserId = HttpContext.Session.GetInt32("UserId") ?? 0;

                _activityLogService.Log(
                    userId: currentUserId,
                    projectId: task.ProjectId,
                    taskId: task.TaskId,
                    actionType: "TASK_ASSIGNED",
                    message: $"assigned task \"{task.Title}\" to {assignedUser.FullName}",
                    createdAt: DateTime.Now,
                    targetUserId: assignedUser.UserId
                );
                await _taskHistoryService.AddAsync(
                task.TaskId,
                currentUserId,
                "TASK_ASSIGNED",
                 $"assign task for {assignedUser.FullName}"
    );

                await _context.SaveChangesAsync();

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
                return RedirectToAction("AccessDeny", "Error", new { returnUrl = HttpContext.Request.Path });
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
                return RedirectToAction("AccessDeny", "Error", new { returnUrl = HttpContext.Request.Path });
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
                // Validate: Deadline > thời gian hiện tại
                if (model.Deadline.HasValue && model.Deadline.Value < DateTime.Now)
                {
                    ModelState.AddModelError("Deadline", "Deadline cannot be in the past");
                }

                // Validate: Task deadline < project deadline
                if (model.Deadline.HasValue && model.Deadline.Value > project.Deadline)
                {
                    ModelState.AddModelError(
                        "Deadline",
                        $"Task deadline cannot exceed project deadline ({project.Deadline:dd/MM/yyyy HH:mm})"
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
            // Validate: Parent task cant Complete nếu còn subtask chưa hoàn thành
            var currentTask = await _context.Tasks
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.TaskId == id);

            if (currentTask != null && currentTask.IsParent == true && model.Status == Models.TaskStatus.Completed)
            {
                var hasIncompleteSubTasks = await _taskService.HasIncompleteSubTasksAsync(id);
                if (hasIncompleteSubTasks)
                {
                    ModelState.AddModelError("Status",
                        "Cannot mark parent task as Completed while there are incomplete subtasks.");
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

                // Truyền ProjectDeadline cho view
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
                //Duc Nghiem
                var oldTask = await _context.Tasks
                    .AsNoTracking()
                    .FirstAsync(t => t.TaskId == model.TaskId);

                bool success = await _taskService.UpdateTaskAsync(model, currentUser.UserId);
                //Trung Hieu
                var newTask1 = await _context.Tasks
    .FirstAsync(t => t.TaskId == model.TaskId);

                if (oldTask.Status != newTask1.Status)
                {
                    await _taskHistoryService.AddAsync(
                        newTask1.TaskId,
                        currentUser.UserId,
                        "STATUS_CHANGED",
                        $"change status from {oldTask.Status} → {newTask1.Status}"
                    );
                }
                if (oldTask.Title != newTask1.Title)
                {
                    await _taskHistoryService.AddAsync(
                        newTask1.TaskId,
                        currentUser.UserId,
                        "TITLE_CHANGED",
                        $"change title from \"{oldTask.Title}\" → \"{newTask1.Title}\""
                    );
                }
                if (oldTask.Deadline != newTask1.Deadline)
                {
                    await _taskHistoryService.AddAsync(
                        newTask1.TaskId,
                        currentUser.UserId,
                        "DEADLINE_CHANGED",
                        $"change deadline from {oldTask.Deadline:dd/MM/yyyy} → {newTask1.Deadline:dd/MM/yyyy}"
                    );
                }


                if (!success)
                {
                    TempData["Error"] = "Cant update task.";
                    return RedirectToAction("BacklogUI", "Backlog", new { projectId = model.ProjectId });
                }

                var newTask = await _context.Tasks
                    .FirstAsync(t => t.TaskId == model.TaskId);

                if (oldTask.Title != newTask.Title)
                {
                    _activityLogService.Log(
                        currentUser.UserId,
                        newTask.ProjectId,
                        newTask.TaskId,
                        "TASK_UPDATED",
                        $"changed title from \"{oldTask.Title}\" to \"{newTask.Title}\"",
                        DateTime.Now
                    );
                }

                // 🔹 Status
                if (oldTask.Status != newTask.Status)
                {
                    _activityLogService.Log(
                        currentUser.UserId,
                        newTask.ProjectId,
                        newTask.TaskId,
                        "TASK_UPDATED",
                        $"changed status from {oldTask.Status} to {newTask.Status}",
                        DateTime.Now
                    );
                }

                // 🔹 Deadline
                if (oldTask.Deadline != newTask.Deadline)
                {
                    _activityLogService.Log(
                        currentUser.UserId,
                        newTask.ProjectId,
                        newTask.TaskId,
                        "TASK_UPDATED",
                        $"changed deadline from {oldTask.Deadline:dd/MM/yyyy} to {newTask.Deadline:dd/MM/yyyy}",
                        DateTime.Now
                    );
                }

                // 5️⃣ SAVE LOG
                await _context.SaveChangesAsync();

                await _projectServices.UpdateProjectStatusAsync(model.ProjectId);

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

                // Truyền ProjectDeadline cho view
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
                return RedirectToAction("BacklogUI", "Backlog", new
                {
                    projectId = _context.Tasks
        .Where(t => t.TaskId == taskId)
        .Select(t => t.ProjectId)
        .First()
                });

            var userId = HttpContext.Session.GetInt32("UserId") ?? 0;

            var hasAttachment = await _context.TaskAttachments
        .AnyAsync(a => a.TaskId == taskId);

            if (hasAttachment)
            {
                TempData["Error"] =
                    "This task already has an attachment. Please delete it before uploading a new one.";
                return RedirectToAction("BacklogUI", "Backlog", new
                {
                    projectId = _context.Tasks
        .Where(t => t.TaskId == taskId)
        .Select(t => t.ProjectId)
        .First()
                });
            }

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
                UploadedBy = userId,
                UploadedAt = DateTime.Now
            };

            _context.TaskAttachments.Add(attachment);
            await _context.SaveChangesAsync();

            await _taskHistoryService.AddAsync(
                taskId,
                userId,
                "ATTACHMENT_ADDED",
                $"has add new attachment \"{file.FileName}\""
            );

            _activityLogService.Log(
                    userId: userId,
                    projectId: await _context.Tasks
                        .Where(t => t.TaskId == taskId)
                        .Select(t => t.ProjectId)
                        .FirstOrDefaultAsync(),
                    taskId: taskId,
                    actionType: "ATTACHMENT_ADDED",
                    message: $"Added an attachment",
                    createdAt: DateTime.Now
                    );


            TempData["SuccessMessage"] = "Upload file successfully!";
            return RedirectToAction("BacklogUI", "Backlog", new
            {
                projectId = _context.Tasks
        .Where(t => t.TaskId == taskId)
        .Select(t => t.ProjectId)
        .First()
            });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAttachment(int taskId)
        {
            var userId = HttpContext.Session.GetInt32("UserId") ?? 0;

            var attachment = await _context.TaskAttachments
                .FirstOrDefaultAsync(a => a.TaskId == taskId);

            if (attachment == null)
                return RedirectToAction("BacklogUI", "Backlog", new
                {
                    projectId = _context.Tasks
                        .Where(t => t.TaskId == taskId)
                        .Select(t => t.ProjectId)
                        .First()
                });

            var physicalPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                attachment.FilePath.TrimStart('/')
            );

            if (System.IO.File.Exists(physicalPath))
                System.IO.File.Delete(physicalPath);

            _context.TaskAttachments.Remove(attachment);
            await _context.SaveChangesAsync();
            await _taskHistoryService.AddAsync(
      attachment.TaskId,
      userId,
      "ATTACHMENT_DELETED",
      $"has deleted \"{attachment.FileName}\""
  );

            _activityLogService.Log(
                    userId: userId,
                    projectId: await _context.Tasks
                        .Where(t => t.TaskId == taskId)
                        .Select(t => t.ProjectId)
                        .FirstOrDefaultAsync(),
                    taskId: taskId,
                    actionType: "ATTACHMENT_DELETED",
                    message: $"Deleted an attachment",
                    createdAt: DateTime.Now
                    );


            TempData["SuccessMessage"] = "Delete attachment successfully!";
            return RedirectToAction("BacklogUI", "Backlog", new
            {
                projectId = _context.Tasks
                    .Where(t => t.TaskId == taskId)
                    .Select(t => t.ProjectId)
                    .First()
            });
        }


        [HttpPost]
        public async Task<IActionResult> ReplaceAttachment(int taskId, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return RedirectToAction("BacklogUI", "Backlog", new
                {
                    projectId = _context.Tasks
                        .Where(t => t.TaskId == taskId)
                        .Select(t => t.ProjectId)
                        .First()
                });

            var userId = HttpContext.Session.GetInt32("UserId") ?? 0;

            var oldAttachment = await _context.TaskAttachments
                .FirstOrDefaultAsync(a => a.TaskId == taskId);

            if (oldAttachment != null)
            {
                var oldPhysicalPath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    oldAttachment.FilePath.TrimStart('/')
                );

                if (System.IO.File.Exists(oldPhysicalPath))
                    System.IO.File.Delete(oldPhysicalPath);

                _context.TaskAttachments.Remove(oldAttachment);
                await _context.SaveChangesAsync();
            }

            // === Upload mới (GIỐNG UploadAttachment) ===
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
                FileName = file.FileName,
                FilePath = "/uploads/" + fileName,
                FileType = file.ContentType,
                FileSize = file.Length,
                UploadedBy = userId,
                UploadedAt = DateTime.Now
            };

            _context.TaskAttachments.Add(attachment);
            await _context.SaveChangesAsync();

            await _taskHistoryService.AddAsync(
    attachment.TaskId,
    userId,
    "ATTACHMENT_REPLACED",
    $"has replace file \"{attachment.FileName}\""
);

            _activityLogService.Log(
                    userId: userId,
                    projectId: await _context.Tasks
                        .Where(t => t.TaskId == taskId)
                        .Select(t => t.ProjectId)
                        .FirstOrDefaultAsync(),
                    taskId: taskId,
                    actionType: "ATTACHMENT_REPLACED",
                    message: $"Replaced an attachment",
                    createdAt: DateTime.Now
                    );

            TempData["SuccessMessage"] = "Replace attachment successfully!";
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
                await _taskHistoryService.AddAsync(
                  taskId,
                  userId,
                  "COMMENT_ADDED",
                  "added comment"
                );

                _activityLogService.Log(
                    userId: userId,
                    projectId: await _context.Tasks
                        .Where(t => t.TaskId == taskId)
                        .Select(t => t.ProjectId)
                        .FirstOrDefaultAsync(),
                    taskId: taskId,
                    actionType: "COMMENT_ADDED",
                    message: $"Added a comment: {content}",
                    createdAt: DateTime.Now
                    );

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
        [HttpPost]
        public async Task<IActionResult> UpdateComment([FromBody] UpdateCommentVM model)
        {
            var userId = HttpContext.Session.GetInt32("UserId") ?? 0;

            var comment = _context.Comments
                .FirstOrDefault(c => c.CommentId == model.CommentId);

            if (comment == null) return NotFound();

            if (comment.UserId != userId)
                return Forbid();

            comment.Content = model.Content;
            _context.SaveChanges();
            await _taskHistoryService.AddAsync(
                 comment.TaskId,
                 userId,
                 "COMMENT_UPDATED",
                 "has updated a comment"
             );


            _activityLogService.Log(
                userId: userId,
                projectId: await _context.Tasks
                    .Where(t => t.TaskId == comment.TaskId)
                    .Select(t => t.ProjectId)
                    .FirstOrDefaultAsync(),
                taskId: comment.TaskId,
                actionType: "COMMENT_EDITED",
                message: $"Update a comment",
                createdAt: DateTime.Now
                );

            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteComment([FromBody] DeleteCommentRequest req)
        {
            var userId = HttpContext.Session.GetInt32("UserId") ?? 0;

            var comment = _context.Comments
                .FirstOrDefault(c => c.CommentId == req.CommentId);

            if (comment == null || comment.UserId != userId)
                return Forbid();

            _context.Comments.Remove(comment);
            _context.SaveChanges();
            await _taskHistoryService.AddAsync(
            comment.TaskId,
            userId,
            "COMMENT_DELETED",
             "has deleted a comment"
  );

            _activityLogService.Log(
                userId: userId,
                projectId: await _context.Tasks
                    .Where(t => t.TaskId == comment.TaskId)
                    .Select(t => t.ProjectId)
                    .FirstOrDefaultAsync(),
                taskId: comment.TaskId,
                actionType: "COMMENT_DELETED",
                message: $"Delete a comment",
                createdAt: DateTime.Now
                );


            return Ok();
        }

    }
}
