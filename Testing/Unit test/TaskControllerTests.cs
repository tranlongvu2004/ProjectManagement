using Microsoft.AspNetCore.Mvc;
using Moq;
using PorjectManagement.Controllers;
using PorjectManagement.Models;
using PorjectManagement.Service.Interface;
using PorjectManagement.ViewModels;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace PorjectManagement.Tests.Controllers
{
    public class TaskControllerTests
    {
        private readonly Mock<ITaskService> _taskServiceMock;
        private readonly Mock<IUserProjectService> _userProjectServiceMock;
        private readonly Mock<ICommentService> _commentServiceMock;
        private readonly LabProjectManagementContext _context;
        private readonly TaskController _controller;

        public TaskControllerTests()
        {
            _taskServiceMock = new Mock<ITaskService>();
            _userProjectServiceMock = new Mock<IUserProjectService>();
            _commentServiceMock = new Mock<ICommentService>();

            // ✅ Setup InMemory Database
            var options = new DbContextOptionsBuilder<LabProjectManagementContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new LabProjectManagementContext(options);

            // Seed test user
            _context.Users.Add(new User
            {
                UserId = 1,
                FullName = "Test User",
                Email = "test@example.com",
                PasswordHash = "hash",
                RoleId = 2
            });
            _context.SaveChanges();

            _controller = new TaskController(
                _taskServiceMock.Object,
                _userProjectServiceMock.Object,
                _commentServiceMock.Object,
                _context
            );

            var httpContext = new DefaultHttpContext();
            httpContext.Session = new TestSession();
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
                .Returns(Mock.Of<IProjectServices>());
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
            // Arrange
            int projectId = 1;

            var users = new List<User>
            {
                new User { UserId = 1, FullName = "Test User" }
            };

            var parentTasks = new List<PorjectManagement.Models.Task>
            {
                new PorjectManagement.Models.Task
                {
                    TaskId = 10,
                    Title = "Parent Task 1"
                }
            };

            _userProjectServiceMock
                .Setup(x => x.GetUsersByProjectIdAsync(projectId))
                .ReturnsAsync(users);

            _taskServiceMock
                .Setup(x => x.GetParentTasksByProjectAsync(projectId))
                .ReturnsAsync(parentTasks);

            // Act
            var result = await _controller.CreateTask(projectId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<TaskCreateViewModel>(viewResult.Model);

            Assert.Equal(projectId, model.ProjectId);
            Assert.Single(model.ProjectMembers);
            Assert.Single(model.ParentTasks);
        }

        [Fact]
        public async System.Threading.Tasks.Task CreateTask_Get_ProjectIdFromReferer_ReturnsView()
        {
            // Arrange
            var users = new List<User>
            {
                new User { UserId = 1, FullName = "User A" }
            };

            _userProjectServiceMock
                .Setup(x => x.GetUsersByProjectIdAsync(1))
                .ReturnsAsync(users);

            _taskServiceMock
                .Setup(x => x.GetParentTasksByProjectAsync(1))
                .ReturnsAsync(new List<Models.Task>());

            var httpContext = _controller.ControllerContext.HttpContext;
            httpContext.Request.Headers["Referer"] =
                "http://localhost/Task/CreateTask?projectId=1";

            // Act
            var result = await _controller.CreateTask((int?)null);

            // Assert
            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<TaskCreateViewModel>(view.Model);
            Assert.Equal(1, model.ProjectId);
        }

        [Fact]
        public async System.Threading.Tasks.Task CreateTask_Get_NoProjectId_NoReferer_ReturnsViewWithDropdown()
        {
            // Arrange
            _userProjectServiceMock
                .Setup(x => x.GetAllProjectsAsync())
                .ReturnsAsync(new List<Project>
                {
                    new Project { ProjectId = 1, ProjectName = "P1" }
                });

            // Act
            var result = await _controller.CreateTask((int?)null);

            // Assert
            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<TaskCreateViewModel>(view.Model);
            Assert.NotNull(view.ViewData["ProjectList"]);
        }

        #endregion

        #region Create Task - POST Tests

        [Fact]
        public async System.Threading.Tasks.Task CreateTask_Post_ValidModel_RedirectsToBacklog()
        {
            // Arrange
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
                .Setup(x => x.CreateTaskAsync(It.IsAny<Models.Task>()))
                .ReturnsAsync(10);

            _taskServiceMock
                .Setup(x => x.AssignUsersToTaskAsync(10, model.SelectedUserIds))
                .Returns(System.Threading.Tasks.Task.CompletedTask);

            // Act
            var result = await _controller.CreateTask(model);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("BacklogUI", redirect.ActionName);
            Assert.Equal("Backlog", redirect.ControllerName);
        }

        [Fact]
        public async System.Threading.Tasks.Task CreateTask_Post_InvalidModel_ReturnsView()
        {
            // Arrange
            var model = new TaskCreateViewModel
            {
                ProjectId = 1,
                Deadline = DateTime.Now.AddDays(-1) // invalid
            };

            _userProjectServiceMock
                .Setup(x => x.GetAllProjectsAsync())
                .ReturnsAsync(new List<Project>());

            _userProjectServiceMock
                .Setup(x => x.GetUsersByProjectIdAsync(1))
                .ReturnsAsync(new List<User>());

            // Act
            var result = await _controller.CreateTask(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
        }

        #endregion

        #region Assign Task Tests

        [Fact]
        public async System.Threading.Tasks.Task Assign_Get_TaskExists_ReturnsView()
        {
            // Arrange
            var vm = new TaskAssignViewModel
            {
                TaskId = 1,
                ProjectId = 1,
                Users = new List<UserListItemVM>()
            };

            _taskServiceMock
                .Setup(x => x.GetAssignTaskDataAsync(1))
                .ReturnsAsync(vm);

            // Act
            var result = await _controller.Assign(1);

            // Assert
            var view = Assert.IsType<ViewResult>(result);
            Assert.Equal(vm, view.Model);
        }

        [Fact]
        public async System.Threading.Tasks.Task Assign_Post_ValidUser_Redirects()
        {
            // Arrange
            _taskServiceMock.Reset();

            var model = new TaskAssignViewModel
            {
                TaskId = 1,
                SelectedUserId = 2
            };

            _taskServiceMock
                .Setup(x => x.AssignTaskAsync(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.Assign(model);

            // Assert
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
            // Arrange
            var model = new TaskAssignViewModel
            {
                TaskId = 1,
                SelectedUserId = 2
            };

            _taskServiceMock
                .Setup(x => x.AssignTaskAsync(1, 2))
                .ThrowsAsync(new Exception("This intern already assigned for another task"));

            _taskServiceMock
                .Setup(x => x.GetAssignTaskDataAsync(1))
                .ReturnsAsync(new TaskAssignViewModel());

            // Act
            var result = await _controller.Assign(model);

            // Assert
            var view = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
        }

        [Fact]
        public async System.Threading.Tasks.Task Assign_Post_NoUserSelected_ReturnsViewWithError()
        {
            // Arrange
            var model = new TaskAssignViewModel
            {
                TaskId = 1,
                SelectedUserId = 0
            };

            _taskServiceMock
                .Setup(x => x.GetAssignTaskDataAsync(1))
                .ReturnsAsync(new TaskAssignViewModel());

            // Act
            var result = await _controller.Assign(model);

            // Assert
            var view = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
        }

        #endregion

        #region Edit Task - GET Tests

        [Fact]
        public async System.Threading.Tasks.Task Edit_Get_NotLoggedIn_RedirectsToLogin()
        {
            // Arrange
            _controller.HttpContext.Session.Clear();

            // Act
            var result = await _controller.Edit(1);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirect.ActionName);
            Assert.Equal("User", redirect.ControllerName);
        }

        [Fact]
        public async System.Threading.Tasks.Task Edit_Get_NoUserEmail_RedirectsToHome()
        {
            // Arrange
            _controller.HttpContext.Session.SetInt32("UserId", 1);
            _controller.HttpContext.Session.Remove("UserEmail");

            // Act
            var result = await _controller.Edit(1);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("Home", redirect.ControllerName);
            Assert.Equal("Không tìm thấy thông tin đăng nhập.", _controller.TempData["Error"]);
        }

        [Fact]
        public async System.Threading.Tasks.Task Edit_Get_UserNotFound_RedirectsToHome()
        {
            // Arrange
            _controller.HttpContext.Session.SetString("UserEmail", "notfound@example.com");

            // Act
            var result = await _controller.Edit(1);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("Home", redirect.ControllerName);
            Assert.Equal("Không tìm thấy thông tin đăng nhập.", _controller.TempData["Error"]);
        }

        [Fact]
        public async System.Threading.Tasks.Task Edit_Get_TaskNotFound_RedirectsToHome()
        {
            // Arrange
            _taskServiceMock
                .Setup(x => x.GetTaskForEditAsync(1, 1))
                .ReturnsAsync((TaskEditViewModel?)null);

            // Act
            var result = await _controller.Edit(1);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("Home", redirect.ControllerName);
            Assert.Equal("Không tìm thấy task hoặc bạn không có quyền chỉnh sửa.", _controller.TempData["Error"]);
        }

        [Fact]
        public async System.Threading.Tasks.Task Edit_Get_ValidTask_ReturnsViewWithModel()
        {
            // Arrange
            var taskModel = new TaskEditViewModel
            {
                TaskId = 1,
                ProjectId = 1,
                Title = "Test Task",
                Description = "Test Description",
                Priority = TaskPriority.High,
                Status = Models.TaskStatus.ToDo,
                Deadline = DateTime.Now.AddDays(5),
                ProjectMembers = new List<User>
                {
                    new User { UserId = 2, FullName = "Member 1" }
                }
            };

            _taskServiceMock
                .Setup(x => x.GetTaskForEditAsync(1, 1))
                .ReturnsAsync(taskModel);

            // Act
            var result = await _controller.Edit(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<TaskEditViewModel>(viewResult.Model);
            Assert.Equal(1, model.TaskId);
            Assert.Equal("Test Task", model.Title);
        }

        #endregion

        #region Edit Task - POST Tests

        [Fact]
        public async System.Threading.Tasks.Task Edit_Post_NotLoggedIn_RedirectsToLogin()
        {
            // Arrange
            _controller.HttpContext.Session.Clear();
            var model = new TaskEditViewModel { TaskId = 1 };

            // Act
            var result = await _controller.Edit(1, model);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirect.ActionName);
            Assert.Equal("User", redirect.ControllerName);
        }

        [Fact]
        public async System.Threading.Tasks.Task Edit_Post_IdMismatch_RedirectsToHome()
        {
            // Arrange
            var model = new TaskEditViewModel { TaskId = 5 };

            // Act
            var result = await _controller.Edit(1, model); // id = 1, but model.TaskId = 5

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("Home", redirect.ControllerName);
            Assert.Equal("Dữ liệu không hợp lệ.", _controller.TempData["Error"]);
        }

        [Fact]
        public async System.Threading.Tasks.Task Edit_Post_DeadlineInPast_ReturnsViewWithError()
        {
            // Arrange
            var model = new TaskEditViewModel
            {
                TaskId = 1,
                ProjectId = 1,
                Title = "Test Task",
                Priority = TaskPriority.Medium,
                Status = Models.TaskStatus.ToDo,
                Deadline = DateTime.Now.AddDays(-5)
            };

            var reloadedModel = new TaskEditViewModel
            {
                TaskId = 1,
                ProjectMembers = new List<User>(),
                CurrentAssignees = new List<TaskAssigneeItem>()
            };

            _taskServiceMock
                .Setup(x => x.GetTaskForEditAsync(1, 1))
                .ReturnsAsync(reloadedModel);

            // Act
            var result = await _controller.Edit(1, model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
            Assert.True(_controller.ModelState.ContainsKey("Deadline"));
        }

        [Fact]
        public async System.Threading.Tasks.Task Edit_Post_InvalidModel_ReturnsViewWithData()
        {
            // Arrange
            var model = new TaskEditViewModel
            {
                TaskId = 1,
                ProjectId = 1,
                Title = "", // Invalid - Required
                Priority = TaskPriority.Medium,
                Status = Models.TaskStatus.ToDo
            };

            _controller.ModelState.AddModelError("Title", "Tên task là bắt buộc");

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
                .Setup(x => x.GetTaskForEditAsync(1, 1))
                .ReturnsAsync(reloadedModel);

            // Act
            var result = await _controller.Edit(1, model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var returnedModel = Assert.IsType<TaskEditViewModel>(viewResult.Model);
            Assert.Single(returnedModel.ProjectMembers);
            Assert.Single(returnedModel.CurrentAssignees);
        }

        [Fact]
        public async System.Threading.Tasks.Task Edit_Post_UpdateFails_RedirectsToBacklog()
        {
            // Arrange
            var model = new TaskEditViewModel
            {
                TaskId = 1,
                ProjectId = 1,
                Title = "Updated Task",
                Priority = TaskPriority.High,
                Status = Models.TaskStatus.Doing,
                Deadline = DateTime.Now.AddDays(10)
            };

            _taskServiceMock
                .Setup(x => x.UpdateTaskAsync(model, 1))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.Edit(1, model);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("BacklogUI", redirect.ActionName);
            Assert.Equal("Backlog", redirect.ControllerName);
            Assert.Equal(1, redirect.RouteValues["projectId"]);
            Assert.Equal("Không thể cập nhật task.", _controller.TempData["Error"]);
        }

        [Fact]
        public async System.Threading.Tasks.Task Edit_Post_ValidModel_UpdateSuccess_RedirectsToBacklog()
        {
            // Arrange
            var model = new TaskEditViewModel
            {
                TaskId = 1,
                ProjectId = 1,
                Title = "Updated Task",
                Description = "Updated Description",
                Priority = TaskPriority.High,
                Status = Models.TaskStatus.Doing,
                Deadline = DateTime.Now.AddDays(10),
                SelectedUserIds = new List<int> { 2, 3 }
            };

            _taskServiceMock
                .Setup(x => x.UpdateTaskAsync(model, 1))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.Edit(1, model);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("BacklogUI", redirect.ActionName);
            Assert.Equal("Backlog", redirect.ControllerName);
            Assert.Equal(1, redirect.RouteValues["projectId"]);
            Assert.Equal("Cập nhật task thành công!", _controller.TempData["Success"]);

            _taskServiceMock.Verify(
                x => x.UpdateTaskAsync(model, 1),
                Times.Once
            );
        }

        [Fact]
        public async System.Threading.Tasks.Task Edit_Post_ServiceThrowsException_ReturnsViewWithError()
        {
            // Arrange
            var model = new TaskEditViewModel
            {
                TaskId = 1,
                ProjectId = 1,
                Title = "Updated Task",
                Priority = TaskPriority.High,
                Status = Models.TaskStatus.Doing,
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
                .Setup(x => x.GetTaskForEditAsync(1, 1))
                .ReturnsAsync(reloadedModel);

            // Act
            var result = await _controller.Edit(1, model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
            Assert.Contains("Lỗi: Database error",
                _controller.ModelState[""].Errors[0].ErrorMessage);
        }

        #endregion
    }
}
