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
using Microsoft.AspNetCore.Mvc.ViewFeatures.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;

namespace PorjectManagement.Tests.Controllers
{
    public class TaskControllerTests
    {
        private readonly Mock<ITaskService> _taskServiceMock;
        private readonly Mock<IUserProjectService> _userProjectServiceMock;

        private readonly TaskController _controller;

        public TaskControllerTests()
        {
            _taskServiceMock = new Mock<ITaskService>();
            _userProjectServiceMock = new Mock<IUserProjectService>();

            _controller = new TaskController(
                _taskServiceMock.Object,
                _userProjectServiceMock.Object
            );

            var httpContext = new DefaultHttpContext();

            httpContext.Session = new TestSession();
            httpContext.Session.SetInt32("UserId", 1);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

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

            _controller.TempData = new TempDataDictionary(
                httpContext,
                Mock.Of<ITempDataProvider>()
            );
        }


        [Fact]
        public async System.Threading.Tasks.Task CreateTask_Get_WithProjectId_ReturnsViewWithMembers()
        {
            // Arrange
            int projectId = 1;
            var users = new List<User>
            {
                new User { UserId = 1, FullName = "Test User" }
            };

            _userProjectServiceMock
                .Setup(x => x.GetUsersByProjectIdAsync(projectId))
                .ReturnsAsync(users);

            // Act
            var result = await _controller.CreateTask(projectId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<TaskCreateViewModel>(viewResult.Model);

            Assert.Equal(projectId, model.ProjectId);
            Assert.Single(model.ProjectMembers);
        }


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
            _taskServiceMock.Reset(); 

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
    }
}
