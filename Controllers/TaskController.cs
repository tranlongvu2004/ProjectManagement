using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PorjectManagement.Models;
using PorjectManagement.Service.Interface;
using PorjectManagement.ViewModels;

namespace PorjectManagement.Controllers
{
    public class TaskController : BaseController
    {
        private readonly ITaskService _taskService;
        private readonly IUserProjectService _userProjectService;
        private readonly LabProjectManagementContext _context; 

        public TaskController(
            ITaskService taskService,
            IUserProjectService userProjectService,
            LabProjectManagementContext context)
        {
            _taskService = taskService;
            _userProjectService = userProjectService;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> CreateTask(int? projectId)
        {
            var redirect = RedirectIfNotLoggedIn();
            if (redirect != null) return redirect;

            var vm = new TaskCreateViewModel();

            // Nếu projectId = null → lấy từ URL referrer
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


            // Nếu sau khi detect vẫn null → fallback
            if (!projectId.HasValue)
            {
                // vẫn load dropdown nếu không tìm được projectId
                var projectList = await _userProjectService.GetAllProjectsAsync();
                ViewBag.ProjectList = new SelectList(projectList, "ProjectId", "ProjectName");
                vm.ProjectMembers = new List<PorjectManagement.Models.User>();
                return View(vm);
            }

            // Đến đây chắc chắn có projectId
            vm.ProjectId = projectId.Value;

            // Load thành viên trong project
            vm.ProjectMembers = await _userProjectService.GetUsersByProjectIdAsync(projectId.Value);

            // Ẩn dropdown project trong view
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
            var redirect = RedirectIfNotLoggedIn();
            if (redirect != null) return redirect;
            var project = await _context.Projects
         .FirstOrDefaultAsync(p => p.ProjectId == model.ProjectId);

            if (project == null)
            {
                ModelState.AddModelError("", "Project does not exist");
            }
            else
            {
                // ❌ Deadline quá khứ
                if (model.Deadline.HasValue && model.Deadline.Value < DateTime.Now)
                {
                    ModelState.AddModelError(
                        "Deadline",
                        "Deadline cannot be in the past"
                    );
                }

                // ❌ Deadline vượt project deadline
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
                // load lại project list
                ViewBag.ProjectList = new SelectList(
                    await _userProjectService.GetAllProjectsAsync(),
                    "ProjectId",
                    "ProjectName"
                );

                // load lại member list theo project
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
                CreatedBy = 1, // TODO: thay bằng user đang login
                CreatedAt = DateTime.Now,
                 IsParent = !model.IsSubTask,
                ParentId = model.IsSubTask ? model.ParentTaskId : null
            };

            var newTaskId = await _taskService.CreateTaskAsync(task);

            // 3️⃣ Sau đó mới assign user vào task
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
            var redirect = RedirectIfNotLoggedIn();
            if (redirect != null) return redirect;

            var userEmail = HttpContext.Session.GetString("UserEmail");
            var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
            
            if (currentUser == null)
            {
                TempData["Error"] = "Không tìm thấy thông tin đăng nhập.";
                return RedirectToAction("Index", "Home");
            }

            var model = await _taskService.GetTaskForEditAsync(id, currentUser.UserId);
            
            if (model == null)
            {
                TempData["Error"] = "Không tìm thấy task hoặc bạn không có quyền chỉnh sửa.";
                return RedirectToAction("Index", "Home");
            }

            return View(model);
        }

        // POST: /Task/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TaskEditViewModel model)
        {
            var redirect = RedirectIfNotLoggedIn();
            if (redirect != null) return redirect;

            if (id != model.TaskId)
            {
                TempData["Error"] = "Dữ liệu không hợp lệ.";
                return RedirectToAction("Index", "Home");
            }

            if (model.Deadline.HasValue && model.Deadline.Value < DateTime.Now)
            {
                ModelState.AddModelError("Deadline", "Deadline không được ở quá khứ");
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
                return View(model);
            }

            try
            {
                var userEmail = HttpContext.Session.GetString("UserEmail");
                var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
                
                bool success = await _taskService.UpdateTaskAsync(model, currentUser.UserId);
                
                if (!success)
                {
                    TempData["Error"] = "Không thể cập nhật task.";
                    return RedirectToAction("BacklogUI", "Backlog", new { projectId = model.ProjectId });
                }

                TempData["Success"] = "Cập nhật task thành công!";
                return RedirectToAction("BacklogUI", "Backlog", new { projectId = model.ProjectId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Lỗi: {ex.Message}");
                var reloadedModel = await _taskService.GetTaskForEditAsync(id, 1);
                if (reloadedModel != null)
                {
                    model.ProjectMembers = reloadedModel.ProjectMembers;
                    model.CurrentAssignees = reloadedModel.CurrentAssignees;
                }
                return View(model);
            }
        }
    }
}
