using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PorjectManagement.Service.Interface;
using PorjectManagement.ViewModels;

namespace PorjectManagement.Controllers
{
    public class TaskController : BaseController
    {
        private readonly ITaskService _taskService;
        private readonly IUserProjectService _userProjectService;

        public TaskController(
            ITaskService taskService,
            IUserProjectService userProjectService)
        {
            _taskService = taskService;
            _userProjectService = userProjectService;
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

            return View(vm);
        }



        [HttpPost]
        public async Task<IActionResult> CreateTask(TaskCreateViewModel model)
        {
            var redirect = RedirectIfNotLoggedIn();
            if (redirect != null) return redirect;
            if (model.Deadline < DateTime.Now)
            {
                ModelState.AddModelError("Deadline", "Deadline không được là thời gian trong quá khứ");
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
                CreatedAt = DateTime.Now
            };

            var newTaskId = await _taskService.CreateTaskAsync(task);

            // 3️⃣ Sau đó mới assign user vào task
            if (model.SelectedUserIds != null && model.SelectedUserIds.Any())
                await _taskService.AssignUsersToTaskAsync(newTaskId, model.SelectedUserIds);
            TempData["SuccessMessage"] = "Tạo task thành công!";

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
                ModelState.AddModelError("", "Bạn phải chọn 1 thành viên.");
                model = await _taskService.GetAssignTaskDataAsync(model.TaskId);
                return View(model);
            }

            await _taskService.AssignTaskAsync(model.TaskId, model.SelectedUserId);

            TempData["Success"] = "Giao công việc thành công!";
            return RedirectToAction("Assign", new { id = model.TaskId });
        }
    }
}
    

