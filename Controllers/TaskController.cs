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

            return View(vm);
        }



        [HttpPost]
        public async Task<IActionResult> CreateTask(TaskCreateViewModel model)
        {
            var redirect = RedirectIfNotLoggedIn();
            if (redirect != null) return redirect;
            if (model.Deadline < DateTime.Now)
            {
                ModelState.AddModelError("Deadline", "Deadline cannot be the past");
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

            var task = new Models.Task
            {
                ProjectId = model.ProjectId,
                Title = model.Title,
                Description = model.Description,
                Priority = model.Priority,
                Status = Models.TaskStatus.ToDo,
                Deadline = model.Deadline,
                CreatedBy = 1,
                CreatedAt = DateTime.Now
            };

            var newTaskId = await _taskService.CreateTaskAsync(task);

            // 3️⃣ Sau đó mới assign user vào task
            if (model.SelectedUserIds != null && model.SelectedUserIds.Any())
                await _taskService.AssignUsersToTaskAsync(newTaskId, model.SelectedUserIds);
            TempData["SuccessMessage"] = "Create task successfully!";

            return RedirectToAction("BacklogUI", "Backlog", new { projectId = model.ProjectId });
        }
        public async Task<IActionResult> Assign(int id)
        {
            var vm = await _taskService.GetAssignTaskDataAsync(id);
            if (vm == null) return NotFound();

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
                TempData["Success"] = "Task assigned successfully!";
                return RedirectToAction("Assign", new { id = model.TaskId });
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
    

