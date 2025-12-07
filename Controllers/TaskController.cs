using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PorjectManagement.Service.Interface;
using PorjectManagement.ViewModels;

namespace PorjectManagement.Controllers
{
    public class TaskController : Controller
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

        // ==========================
        // GET: Create Task
        // ==========================
        [HttpGet]
        public async Task<IActionResult> CreateTask(int? projectId)
        {
            var vm = new TaskCreateViewModel();

            // Load toàn bộ project cho dropdown
            var projectList = await _userProjectService.GetAllProjectsAsync();
            ViewBag.ProjectList = new SelectList(projectList, "ProjectId", "ProjectName");

            // Nếu có projectId → load thành viên
            if (projectId.HasValue)
            {
                vm.ProjectId = projectId.Value;
                vm.ProjectMembers = await _userProjectService.GetUsersByProjectIdAsync(projectId.Value);
            }
            else
            {
                vm.ProjectMembers = new List<PorjectManagement.Models.User>();
            }

            return View(vm);
        }

        // ==========================
        // POST: Create Task
        // ==========================
        [HttpPost]
        public async Task<IActionResult> CreateTask(TaskCreateViewModel model)
        {
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

            // 4️⃣ Trả lại message thành công
            ViewBag.SuccessMessage = "Tạo task thành công!";

            // Load lại member list
            model.ProjectMembers = await _userProjectService.GetUsersByProjectIdAsync(model.ProjectId);

            // Xóa nội dung form sau khi tạo
            ModelState.Clear();
            model.Title = "";
            model.Description = "";

            return View(model);
        }
    }
}
