using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using PorjectManagement.Controllers;
using PorjectManagement.Models;
using PorjectManagement.Service.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace PorjectManagement.Tests.Controllers
{
    public class BacklogControllerTest
    {
        private readonly Mock<IProjectServices> _mockProjectService;
        private readonly Mock<IUserProjectService> _mockUserProjectService;
        private readonly Mock<ICommentService> _mockCommentService;
        private readonly LabProjectManagementContext _context;
        private readonly BacklogController _controller;

        public BacklogControllerTest()
        {
            _mockProjectService = new Mock<IProjectServices>();
            _mockUserProjectService = new Mock<IUserProjectService>();
            _mockCommentService = new Mock<ICommentService>();

            var options = new DbContextOptionsBuilder<LabProjectManagementContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new LabProjectManagementContext(options);

            _controller = new BacklogController(
                _context,
                _mockProjectService.Object,
                _mockUserProjectService.Object,
                _mockCommentService.Object
            );

            // Fake HttpContext + Session
            var httpContext = new DefaultHttpContext();
            httpContext.Session = new TestSession();
            httpContext.Session.SetInt32("UserId", 1);
            httpContext.Session.SetInt32("RoleId", 2);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }

        // BacklogUI TESTS

        [Fact]
        public async System.Threading.Tasks.Task BacklogUI_UserNotLoggedIn_RedirectsToLogin()
        {
            // Arrange
            _controller.ControllerContext.HttpContext.Session.Clear();

            // Act
            var result = await _controller.BacklogUI(1);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirect.ActionName);
            Assert.Equal("User", redirect.ControllerName);
        }

        [Fact]
        public async System.Threading.Tasks.Task BacklogUI_ReturnsView_WithParentAndSubTasks()
        {
            // Arrange
            _mockUserProjectService
                .Setup(x => x.IsleaderOfProject(1, 1))
                .Returns(true);

            _context.Tasks.AddRange(
                new PorjectManagement.Models.Task
                {
                    TaskId = 1,
                    Title = "Parent Task",
                    ProjectId = 1,
                    IsParent = true,
                    ParentId = null,
                    CreatedAt = DateTime.Now
                },
                new PorjectManagement.Models.Task
                {
                    TaskId = 2,
                    Title = "Sub Task",
                    ProjectId = 1,
                    IsParent = false,
                    ParentId = 1,
                    CreatedAt = DateTime.Now
                }
            );
            _context.SaveChanges();

            // Act
            var result = await _controller.BacklogUI(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);

            var parentTasks = Assert.IsAssignableFrom<List<PorjectManagement.Models.Task>>(viewResult.ViewData["ParentTasks"]);
            var subTasks = Assert.IsAssignableFrom<List<PorjectManagement.Models.Task>>(viewResult.ViewData["SubTasks"]);

            Assert.Single(parentTasks);
            Assert.Single(subTasks);
        }

        // DeleteTask TESTS

        [Fact]
        public async System.Threading.Tasks.Task DeleteTask_TaskNotFound_ReturnsNotFound()
        {
            // Arrange
            var request = new DeleteTaskRequest { TaskId = 999 };

            // Act
            var result = await _controller.DeleteTask(request);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }


        [Fact]
        public async System.Threading.Tasks.Task DeleteTask_ValidTask_AddsToRecycleBin_ReturnsOk()
        {
            // Arrange
            _mockProjectService
                .Setup(x => x.UpdateProjectStatusAsync(It.IsAny<int>()))
                .Returns(System.Threading.Tasks.Task.CompletedTask);

            _context.Tasks.Add(new PorjectManagement.Models.Task
            {
                TaskId = 1,
                Title = "Test Task",
                ProjectId = 1,
                Status = Models.TaskStatus.Doing,
                CreatedAt = DateTime.Now
            });
            _context.SaveChanges();

            var request = new DeleteTaskRequest { TaskId = 1 };

            // Act
            var result = await _controller.DeleteTask(request);

            // Assert
            Assert.IsType<OkResult>(result);

            var recycleItem = _context.RecycleBins
                .FirstOrDefault(r => r.EntityType == "Task" && r.EntityId == 1);

            Assert.NotNull(recycleItem);
        }

    }
}
