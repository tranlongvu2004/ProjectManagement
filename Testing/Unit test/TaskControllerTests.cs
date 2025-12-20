using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Moq;
using PorjectManagement.Controllers;
using PorjectManagement.Models;
using PorjectManagement.Service.Interface;
using PorjectManagement.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace PorjectManagement.Tests.Controllers
{
    // Simple in-memory ISession for unit tests
    public class TestSession : ISession
    {
        private readonly Dictionary<string, byte[]> _store = new();

        public IEnumerable<string> Keys => _store.Keys;
        public string Id => Guid.NewGuid().ToString();
        public bool IsAvailable => true;

        public void Clear() => _store.Clear();
        public System.Threading.Tasks.Task CommitAsync(CancellationToken cancellationToken = default) => System.Threading.Tasks.Task.CompletedTask;
        public System.Threading.Tasks.Task LoadAsync(CancellationToken cancellationToken = default) => System.Threading.Tasks.Task.CompletedTask;
        public void Remove(string key) => _store.Remove(key);

        public void Set(string key, byte[] value) => _store[key] = value;
        public bool TryGetValue(string key, out byte[] value) => _store.TryGetValue(key, out value);
    }

    public class TaskControllerTests
    {
        private readonly Mock<ITaskService> _taskServiceMock;
        private readonly Mock<IUserProjectService> _userProjectServiceMock;
        private readonly Mock<ICommentService> _commentServiceMock;
        private readonly Mock<IProjectServices> _projectServicesMock;
        private readonly LabProjectManagementContext _context;
        private readonly TaskController _controller;

        public TaskControllerTests()
        {
            _taskServiceMock = new Mock<ITaskService>();
            _userProjectServiceMock = new Mock<IUserProjectService>();
            _commentServiceMock = new Mock<ICommentService>();
            _projectServicesMock = new Mock<IProjectServices>();

            var options = new DbContextOptionsBuilder<LabProjectManagementContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new LabProjectManagementContext(options);

            // Seed user
            _context.Users.Add(new User
            {
                UserId = 1,
                FullName = "Test User",
                Email = "test@example.com",
                PasswordHash = "hash",
                RoleId = 2
            });

            // Seed project (for validations in Create/Edit)
            _context.Projects.Add(new Project
            {
                ProjectId = 1,
                ProjectName = "P1",
                Deadline = DateTime.Now.AddDays(30)
            });

            // Seed a task for Edit tests when needed
            _context.Tasks.Add(new PorjectManagement.Models.Task
            {
                TaskId = 1,
                ProjectId = 1,
                Title = "Seed Task",
                CreatedAt = DateTime.Now,
                CreatedBy = 1,
                Status = PorjectManagement.Models.TaskStatus.ToDo,
                Priority = TaskPriority.Medium
            });

            _context.SaveChanges();

            // Default mocks never return null (avoid SelectList null crash)
            _userProjectServiceMock
                .Setup(x => x.GetAllProjectsAsync())
                .ReturnsAsync(new List<Project> { new Project { ProjectId = 1, ProjectName = "P1" } });

            _userProjectServiceMock
                .Setup(x => x.GetUsersByProjectIdAsync(It.IsAny<int>()))
                .ReturnsAsync(new List<User>());

            _controller = new TaskController(
                _taskServiceMock.Object,
                _userProjectServiceMock.Object,
                _commentServiceMock.Object,
                _projectServicesMock.Object,
                _context
            );

            var httpContext = new DefaultHttpContext();
            httpContext.Session = new TestSession();

            // ✅ IMPORTANT: set RoleId = 2 by default to pass role checks
            httpContext.Session.SetInt32("RoleId", 2);
            httpContext.Session.SetInt32("UserId", 1);
            httpContext.Session.SetString("UserEmail", "test@example.com");

            httpContext.User = new ClaimsPrincipal(
                new ClaimsIdentity(
                    new[] { new Claim(ClaimTypes.NameIdentifier, "1") },
                    "TestAuth"
                )
            );

            var urlHelperMock = new Mock<IUrlHelper>();
            urlHelperMock
                .Setup(x => x.Action(It.IsAny<UrlActionContext>()))
                .Returns("mocked-url");

            var urlHelperFactoryMock = new Mock<IUrlHelperFactory>();
            urlHelperFactoryMock
                .Setup(f => f.GetUrlHelper(It.IsAny<ActionContext>()))
                .Returns(urlHelperMock.Object);

            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider
                .Setup(x => x.GetService(typeof(IProjectServices)))
                .Returns(_projectServicesMock.Object);
            serviceProvider
                .Setup(x => x.GetService(typeof(IUrlHelperFactory)))
                .Returns(urlHelperFactoryMock.Object);

            httpContext.RequestServices = serviceProvider.Object;

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            _controller.TempData = new TempDataDictionary(
                httpContext,
                Mock.Of<ITempDataProvider>()
            );
        }

        #region Create Task - GET Tests

        [Fact]
        public async System.Threading.Tasks.Task CreateTask_Get_WithProjectId_ReturnsViewWithMembers()
        {
            int projectId = 1;

            var users = new List<User>
            {
                new User { UserId = 1, FullName = "Test User" }
            };

            var parentTasks = new List<PorjectManagement.Models.Task>
            {
                new PorjectManagement.Models.Task { TaskId = 10, Title = "Parent Task 1" }
            };

            _userProjectServiceMock
                .Setup(x => x.GetUsersByProjectIdAsync(projectId))
                .ReturnsAsync(users);

            _taskServiceMock
                .Setup(x => x.GetParentTasksByProjectAsync(projectId))
                .ReturnsAsync(parentTasks);

            var result = await _controller.CreateTask(projectId);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<TaskCreateViewModel>(viewResult.Model);

            Assert.Equal(projectId, model.ProjectId);
            Assert.Single(model.ProjectMembers);
            Assert.Single(model.ParentTasks);
        }

        [Fact]
        public async System.Threading.Tasks.Task CreateTask_Get_ProjectIdFromReferer_ReturnsView()
        {
            var users = new List<User> { new User { UserId = 1, FullName = "User A" } };

            _userProjectServiceMock
                .Setup(x => x.GetUsersByProjectIdAsync(1))
                .ReturnsAsync(users);

            _taskServiceMock
                .Setup(x => x.GetParentTasksByProjectAsync(1))
                .ReturnsAsync(new List<PorjectManagement.Models.Task>());

            var httpContext = _controller.ControllerContext.HttpContext;
            httpContext.Request.Headers["Referer"] =
                "http://localhost/Task/CreateTask?projectId=1";

            var result = await _controller.CreateTask((int?)null);

            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<TaskCreateViewModel>(view.Model);
            Assert.Equal(1, model.ProjectId);
        }

        [Fact]
        public async    System.Threading.Tasks.Task CreateTask_Get_NoProjectId_NoReferer_ReturnsViewWithDropdown()
        {
            // Ensure roleId=2 so it doesn't AccessDeny
            _controller.HttpContext.Session.SetInt32("RoleId", 2);

            var result = await _controller.CreateTask((int?)null);

            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<TaskCreateViewModel>(view.Model);
            Assert.NotNull(view.ViewData["ProjectList"]);
        }

        #endregion

        #region Create Task - POST Tests

        [Fact]
        public async System.Threading.Tasks.Task CreateTask_Post_ValidModel_RedirectsToBacklog()
        {
            // Ensure project exists (seeded in ctor), role ok
            _controller.HttpContext.Session.SetInt32("RoleId", 2);
            _controller.HttpContext.Session.SetInt32("UserId", 1);

            var model = new TaskCreateViewModel
            {
                ProjectId = 1,
                Title = "Test Task",
                Description = "Desc",
                Priority = TaskPriority.Medium,
                Deadline = DateTime.Now.AddDays(1),
                SelectedUserIds = new List<int> { 1, 2 }
            };

            _taskServiceMock
                .Setup(x => x.CreateTaskAsync(It.IsAny<PorjectManagement.Models.Task>()))
                .ReturnsAsync(10);

            _taskServiceMock
                .Setup(x => x.AssignUsersToTaskAsync(10, model.SelectedUserIds))
                .Returns(System.Threading.Tasks.Task.CompletedTask);

            var result = await _controller.CreateTask(model);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("BacklogUI", redirect.ActionName);
            Assert.Equal("Backlog", redirect.ControllerName);
            Assert.Equal(1, redirect.RouteValues["projectId"]);
        }

        [Fact]
        public async System.Threading.Tasks.Task CreateTask_Post_InvalidModel_ReturnsView()
        {
            _controller.HttpContext.Session.SetInt32("RoleId", 2);

            var model = new TaskCreateViewModel
            {
                ProjectId = 1,
                Deadline = DateTime.Now.AddDays(-1) // invalid past
            };

            var result = await _controller.CreateTask(model);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
        }

        #endregion

        #region Assign Task Tests

        [Fact]
        public async System.Threading.Tasks.Task Assign_Get_TaskExists_ReturnsView()
        {
            var vm = new TaskAssignViewModel
            {
                TaskId = 1,
                ProjectId = 1,
                Users = new List<UserListItemVM>()
            };

            _taskServiceMock
                .Setup(x => x.GetAssignTaskDataAsync(1))
                .ReturnsAsync(vm);

            var result = await _controller.Assign(1);

            var view = Assert.IsType<ViewResult>(result);
            Assert.Equal(vm, view.Model);
        }

        [Fact]
        public async System.Threading.Tasks.Task Assign_Post_ValidUser_Redirects()
        {
            _controller.HttpContext.Session.SetInt32("RoleId", 2);

            var model = new TaskAssignViewModel
            {
                TaskId = 1,
                SelectedUserId = 2
            };

            _taskServiceMock
                .Setup(x => x.AssignTaskAsync(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(true);

            var result = await _controller.Assign(model);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Assign", redirect.ActionName);

            _taskServiceMock.Verify(
                x => x.AssignTaskAsync(1, 2),
                Times.Once
            );
        }

        [Fact]
        public async System.Threading.Tasks.Task Assign_Post_DuplicateUser_ReturnsViewWithError()
        {
            _controller.HttpContext.Session.SetInt32("RoleId", 2);

            var model = new TaskAssignViewModel
            {
                TaskId = 1,
                SelectedUserId = 2
            };

            _taskServiceMock
                .Setup(x => x.AssignTaskAsync(It.IsAny<int>(), It.IsAny<int>()))
                .ThrowsAsync(new Exception("This intern already assigned for another task"));

            _taskServiceMock
                .Setup(x => x.GetAssignTaskDataAsync(1))
                .ReturnsAsync(new TaskAssignViewModel());

            var result = await _controller.Assign(model);

            var view = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
        }

        [Fact]
        public async System.Threading.Tasks.Task Assign_Post_NoUserSelected_ReturnsViewWithError()
        {
            _controller.HttpContext.Session.SetInt32("RoleId", 2);

            var model = new TaskAssignViewModel
            {
                TaskId = 1,
                SelectedUserId = 0
            };

            _taskServiceMock
                .Setup(x => x.GetAssignTaskDataAsync(1))
                .ReturnsAsync(new TaskAssignViewModel());

            var result = await _controller.Assign(model);

            var view = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
        }

        #endregion

        #region Edit Task - GET Tests

        [Fact]
        public async System.Threading.Tasks.Task Edit_Get_NotLoggedIn_RedirectsToAccessDeny()
        {
            // With current controller: role check happens before "login" check.
            _controller.HttpContext.Session.Clear();

            var result = await _controller.Edit(1);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("AccessDeny", redirect.ActionName);
            Assert.Equal("Error", redirect.ControllerName);
        }

        [Fact]
        public async System.Threading.Tasks.Task Edit_Get_NoUserEmail_RedirectsToHome()
        {
            _controller.HttpContext.Session.SetInt32("RoleId", 2);
            _controller.HttpContext.Session.SetInt32("UserId", 1);
            _controller.HttpContext.Session.Remove("UserEmail");

            var result = await _controller.Edit(1);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("Home", redirect.ControllerName);
            Assert.Equal("Login information not found.", _controller.TempData["Error"]);
        }

        [Fact]
        public async System.Threading.Tasks.Task Edit_Get_UserNotFound_RedirectsToHome()
        {
            _controller.HttpContext.Session.SetInt32("RoleId", 2);
            _controller.HttpContext.Session.SetString("UserEmail", "notfound@example.com");

            var result = await _controller.Edit(1);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("Home", redirect.ControllerName);
            Assert.Equal("Login information not found.", _controller.TempData["Error"]);
        }

        [Fact]
        public async    System.Threading.Tasks.Task Edit_Get_TaskNotFound_RedirectsToHome()
        {
            _controller.HttpContext.Session.SetInt32("RoleId", 2);
            _controller.HttpContext.Session.SetString("UserEmail", "test@example.com");

            _taskServiceMock
                .Setup(x => x.GetTaskForEditAsync(1, 1))
                .ReturnsAsync((TaskEditViewModel?)null);

            var result = await _controller.Edit(1);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("Home", redirect.ControllerName);
            Assert.Equal("Task not found, or you do not have editing permissions.", _controller.TempData["Error"]);
        }

        [Fact]
        public async System.Threading.Tasks.Task Edit_Get_ValidTask_ReturnsViewWithModel()
        {
            _controller.HttpContext.Session.SetInt32("RoleId", 2);
            _controller.HttpContext.Session.SetString("UserEmail", "test@example.com");

            var taskModel = new TaskEditViewModel
            {
                TaskId = 1,
                ProjectId = 1,
                Title = "Test Task",
                Description = "Test Description",
                Priority = TaskPriority.High,
                Status = PorjectManagement.Models.TaskStatus.ToDo,
                Deadline = DateTime.Now.AddDays(5),
                ProjectMembers = new List<User>
                {
                    new User { UserId = 2, FullName = "Member 1" }
                }
            };

            _taskServiceMock
                .Setup(x => x.GetTaskForEditAsync(1, 1))
                .ReturnsAsync(taskModel);

            var result = await _controller.Edit(1);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<TaskEditViewModel>(viewResult.Model);
            Assert.Equal(1, model.TaskId);
            Assert.Equal("Test Task", model.Title);
        }

        #endregion

        #region Edit Task - POST Tests

        [Fact]
        public async System.Threading.Tasks.Task Edit_Post_NotLoggedIn_RedirectsToAccessDeny()
        {
            _controller.HttpContext.Session.Clear();

            var model = new TaskEditViewModel { TaskId = 1 };

            var result = await _controller.Edit(1, model);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("AccessDeny", redirect.ActionName);
            Assert.Equal("Error", redirect.ControllerName);
        }

        [Fact]
        public async System.Threading.Tasks.Task Edit_Post_IdMismatch_RedirectsToHome()
        {
            _controller.HttpContext.Session.SetInt32("RoleId", 2);

            var model = new TaskEditViewModel { TaskId = 5 };

            var result = await _controller.Edit(1, model);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("Home", redirect.ControllerName);
            Assert.Equal("Data not valid.", _controller.TempData["Error"]);
        }

        [Fact]
        public async System.Threading.Tasks.Task Edit_Post_DeadlineInPast_ReturnsViewWithError()
        {
            _controller.HttpContext.Session.SetInt32("RoleId", 2);
            _controller.HttpContext.Session.SetString("UserEmail", "test@example.com");

            var model = new TaskEditViewModel
            {
                TaskId = 1,
                ProjectId = 1,
                Title = "Test Task",
                Priority = TaskPriority.Medium,
                Status = PorjectManagement.Models.TaskStatus.ToDo,
                Deadline = DateTime.Now.AddDays(-5)
            };

            var reloadedModel = new TaskEditViewModel
            {
                TaskId = 1,
                ProjectMembers = new List<User>(),
                CurrentAssignees = new List<TaskAssigneeItem>()
            };

            _taskServiceMock
                .Setup(x => x.GetTaskForEditAsync(1, It.IsAny<int>()))
                .ReturnsAsync(reloadedModel);

            var result = await _controller.Edit(1, model);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
            Assert.True(_controller.ModelState.ContainsKey("Deadline"));
        }

        [Fact]
        public async System.Threading.Tasks.Task Edit_Post_InvalidModel_ReturnsViewWithData()
        {
            _controller.HttpContext.Session.SetInt32("RoleId", 2);
            _controller.HttpContext.Session.SetString("UserEmail", "test@example.com");

            var model = new TaskEditViewModel
            {
                TaskId = 1,
                ProjectId = 1,
                Title = "",
                Priority = TaskPriority.Medium,
                Status = PorjectManagement.Models.TaskStatus.ToDo
            };

            _controller.ModelState.AddModelError("Title", "Title is required");

            var reloadedModel = new TaskEditViewModel
            {
                TaskId = 1,
                ProjectMembers = new List<User>
                {
                    new User { UserId = 2, FullName = "Member 1" }
                },
                CurrentAssignees = new List<TaskAssigneeItem>
                {
                    new TaskAssigneeItem { UserId = 2, FullName = "Member 1" }
                }
            };

            _taskServiceMock
                .Setup(x => x.GetTaskForEditAsync(1, It.IsAny<int>()))
                .ReturnsAsync(reloadedModel);

            var result = await _controller.Edit(1, model);

            var viewResult = Assert.IsType<ViewResult>(result);
            var returnedModel = Assert.IsType<TaskEditViewModel>(viewResult.Model);

            Assert.Single(returnedModel.ProjectMembers);
            Assert.Single(returnedModel.CurrentAssignees);
        }

        [Fact]
        public async System.Threading.Tasks.Task Edit_Post_UpdateFails_RedirectsToBacklog()
        {
            _controller.HttpContext.Session.SetInt32("RoleId", 2);
            _controller.HttpContext.Session.SetString("UserEmail", "test@example.com");

            var model = new TaskEditViewModel
            {
                TaskId = 1,
                ProjectId = 1,
                Title = "Updated Task",
                Priority = TaskPriority.High,
                Status = PorjectManagement.Models.TaskStatus.Doing,
                Deadline = DateTime.Now.AddDays(10)
            };

            _taskServiceMock
                .Setup(x => x.UpdateTaskAsync(model, 1))
                .ReturnsAsync(false);

            var result = await _controller.Edit(1, model);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("BacklogUI", redirect.ActionName);
            Assert.Equal("Backlog", redirect.ControllerName);
            Assert.Equal(1, redirect.RouteValues["projectId"]);
            Assert.Equal("Cant update task.", _controller.TempData["Error"]);
        }

        [Fact]
        public async System.Threading.Tasks.Task Edit_Post_ValidModel_UpdateSuccess_RedirectsToBacklog()
        {
            _controller.HttpContext.Session.SetInt32("RoleId", 2);
            _controller.HttpContext.Session.SetString("UserEmail", "test@example.com");

            var model = new TaskEditViewModel
            {
                TaskId = 1,
                ProjectId = 1,
                Title = "Updated Task",
                Description = "Updated Description",
                Priority = TaskPriority.High,
                Status = PorjectManagement.Models.TaskStatus.Doing,
                Deadline = DateTime.Now.AddDays(10),
                SelectedUserIds = new List<int> { 2, 3 }
            };

            _taskServiceMock
                .Setup(x => x.UpdateTaskAsync(model, 1))
                .ReturnsAsync(true);

            var result = await _controller.Edit(1, model);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("BacklogUI", redirect.ActionName);
            Assert.Equal("Backlog", redirect.ControllerName);
            Assert.Equal(1, redirect.RouteValues["projectId"]);
            Assert.Equal("Update task successful!", _controller.TempData["Success"]);

            _taskServiceMock.Verify(x => x.UpdateTaskAsync(model, 1), Times.Once);
        }

        [Fact]
        public async System.Threading.Tasks.Task Edit_Post_ServiceThrowsException_ReturnsViewWithError()
        {
            _controller.HttpContext.Session.SetInt32("RoleId", 2);
            _controller.HttpContext.Session.SetString("UserEmail", "test@example.com");

            var model = new TaskEditViewModel
            {
                TaskId = 1,
                ProjectId = 1,
                Title = "Updated Task",
                Priority = TaskPriority.High,
                Status = PorjectManagement.Models.TaskStatus.Doing,
                Deadline = DateTime.Now.AddDays(10)
            };

            _taskServiceMock
                .Setup(x => x.UpdateTaskAsync(model, 1))
                .ThrowsAsync(new Exception("Database error"));

            var reloadedModel = new TaskEditViewModel
            {
                TaskId = 1,
                ProjectMembers = new List<User>(),
                CurrentAssignees = new List<TaskAssigneeItem>()
            };

            _taskServiceMock
                .Setup(x => x.GetTaskForEditAsync(1, It.IsAny<int>()))
                .ReturnsAsync(reloadedModel);

            var result = await _controller.Edit(1, model);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
            Assert.Contains("Error: Database error", _controller.ModelState[""].Errors[0].ErrorMessage);
        }

        #endregion
    }
}
