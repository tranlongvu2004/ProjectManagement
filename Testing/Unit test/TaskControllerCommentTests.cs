using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Moq;
using PorjectManagement.Controllers;
using PorjectManagement.Models;
using PorjectManagement.Service.Interface;
using PorjectManagement.ViewModels;
using Xunit;

namespace PorjectManagement.Tests.Controllers
{
    public class TaskControllerCommentTests
    {
        private readonly LabProjectManagementContext _context;
        private readonly Mock<ITaskService> _taskServiceMock = new();
        private readonly Mock<IUserProjectService> _userProjectServiceMock = new();
        private readonly Mock<ICommentService> _commentServiceMock = new();
        private readonly Mock<IProjectServices> _projectServicesMock = new();
        private readonly Mock<IActivityLogService> _activityLogServiceMock = new();
        private readonly Mock<ITaskHistoryService> _taskHistoryServiceMock = new();
        private readonly TaskController _controller;

        public TaskControllerCommentTests()
        {
            var options = new DbContextOptionsBuilder<LabProjectManagementContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _context = new LabProjectManagementContext(options);

            // Seed a user and a task for success paths
            _context.Users.Add(new User { UserId = 1, FullName = "User1", Email = "u1@example.com", RoleId = 2, PasswordHash = "hash1" });
            _context.Users.Add(new User { UserId = 2, FullName = "User2", Email = "u2@example.com", RoleId = 2, PasswordHash = "hash2" });
            _context.Projects.Add(new Project { ProjectId = 1, ProjectName = "P1", Deadline = DateTime.Now.AddDays(10) });
            _context.Tasks.Add(new PorjectManagement.Models.Task { TaskId = 100, ProjectId = 1, Title = "T100", CreatedAt = DateTime.Now });
            _context.SaveChanges();

            _controller = new TaskController(
                _taskServiceMock.Object,
                _userProjectServiceMock.Object,
                _commentServiceMock.Object,
                _projectServicesMock.Object,
                _context,
                _activityLogServiceMock.Object,
                _taskHistoryServiceMock.Object
            );

            var httpContext = new DefaultHttpContext();
            httpContext.Session = new TestSession();
            httpContext.Session.SetInt32("RoleId", 2);
            httpContext.Session.SetInt32("UserId", 1);
            httpContext.Session.SetString("UserEmail", "u1@example.com");

            _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
            _controller.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
        }

        [Fact]
        public async System.Threading.Tasks.Task AddComment_PermissionDenied_RedirectsWithError()
        {
            // Arrange: remove session role/user
            _controller.ControllerContext.HttpContext.Session.Clear();

            // Act
            var result = await _controller.AddComment(100, "hello");

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("BacklogUI", redirect.ActionName);
            Assert.Equal("You don't have permission to add comment.", _controller.TempData["Error"]);
        }

        [Fact]
        public async System.Threading.Tasks.Task AddComment_EmptyContent_ReturnsWithError()
        {
            // Arrange: valid session but empty content
            _controller.ControllerContext.HttpContext.Session.SetInt32("RoleId", 2);
            _controller.ControllerContext.HttpContext.Session.SetInt32("UserId", 1);

            // Act
            var result = await _controller.AddComment(100, "   ");

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("BacklogUI", redirect.ActionName);
            Assert.Equal("Comment cannot be empty.", _controller.TempData["Error"]);
        }

        [Fact]
        public async System.Threading.Tasks.Task AddComment_Success_RedirectsToBacklogAndAddsHistory()
        {
            // Arrange
            _commentServiceMock
                .Setup(s => s.AddCommentAsync(100, 1, It.IsAny<string>()))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.AddComment(100, "New comment");

            // Assert redirect to BacklogUI with projectId retrieved from task
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("BacklogUI", redirect.ActionName);
            Assert.Equal("Backlog", redirect.ControllerName);
            Assert.Equal(1, redirect.RouteValues["projectId"]);

            Assert.Equal("Comment added successfully!", _controller.TempData["Success"]);
            _commentServiceMock.Verify(s => s.AddCommentAsync(100, 1, "New comment"), Times.Once);
            _taskHistoryServiceMock.Verify(h => h.AddAsync(100, 1, "COMMENT_ADDED", It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async System.Threading.Tasks.Task UpdateComment_Unauthorized_ReturnsUnauthorized()
        {
            // Arrange: clear user
            _controller.ControllerContext.HttpContext.Session.Remove("UserId");

            var model = new UpdateCommentVM { CommentId = 999, Content = "x" };

            // Act
            var result = await _controller.UpdateComment(model);

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async System.Threading.Tasks.Task UpdateComment_NotFound_ReturnsNotFound()
        {
            // Arrange: ensure user exists but comment doesn't
            _controller.ControllerContext.HttpContext.Session.SetInt32("UserId", 1);
            var model = new UpdateCommentVM { CommentId = 9999, Content = "x" };

            // Act
            var result = await _controller.UpdateComment(model);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async System.Threading.Tasks.Task UpdateComment_Forbid_WhenNotOwner()
        {
            // Arrange: seed comment owned by user 2
            _context.Comments.Add(new Comment { CommentId = 500, TaskId = 100, UserId = 2, Content = "old" });
            _context.SaveChanges();

            _controller.ControllerContext.HttpContext.Session.SetInt32("UserId", 1);
            var model = new UpdateCommentVM { CommentId = 500, Content = "new" };

            // Act
            var result = await _controller.UpdateComment(model);

            // Assert
            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async System.Threading.Tasks.Task DeleteComment_Forbid_WhenNotOwnerOrMissing()
        {
            // Arrange: ensure comment exists but owned by someone else
            _context.Comments.Add(new Comment { CommentId = 600, TaskId = 100, UserId = 2, Content = "to delete" });
            _context.SaveChanges();

            _controller.ControllerContext.HttpContext.Session.SetInt32("UserId", 1);

            var req = new DeleteCommentRequest { CommentId = 600 };

            // Act
            var result = await _controller.DeleteComment(req);

            // Assert
            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async System.Threading.Tasks.Task DeleteComment_Success_RemovesAndReturnsOk()
        {
            // Arrange: seed comment owned by current user
            _context.Comments.Add(new Comment { CommentId = 601, TaskId = 100, UserId = 1, Content = "to delete" });
            _context.SaveChanges();

            _controller.ControllerContext.HttpContext.Session.SetInt32("UserId", 1);
            var req = new DeleteCommentRequest { CommentId = 601 };

            // Act
            var result = await _controller.DeleteComment(req);

            // Assert
            Assert.IsType<OkResult>(result);
            Assert.Null(_context.Comments.FirstOrDefault(c => c.CommentId == 601));
            _taskHistoryServiceMock.Verify(h => h.AddAsync(100, 1, "COMMENT_DELETED", It.IsAny<string>()), Times.Once);
        }
    }
}