using System;
using System.Collections.Generic;
using System.Linq;
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

            SeedData();
        }

        private void SeedData()
        {
            var user1 = new User
            {
                UserId = 1,
                FullName = "User1",
                Email = "u1@example.com",
                PasswordHash = "hash1",
                RoleId = 2
            };

            var user2 = new User
            {
                UserId = 2,
                FullName = "User2",
                Email = "u2@example.com",
                PasswordHash = "hash2",
                RoleId = 2
            };

            var project = new Project
            {
                ProjectId = 1,
                ProjectName = "P1",
                Deadline = DateTime.Now.AddDays(10)
            };

            var task = new PorjectManagement.Models.Task
            {
                TaskId = 100,
                ProjectId = 1,
                Title = "T100",
                CreatedAt = DateTime.Now
            };

            // comments
            var commentOwnedByUser1 = new Comment
            {
                CommentId = 501,
                TaskId = 100,
                UserId = 1,
                Content = "old content"
            };

            var commentOwnedByUser2 = new Comment
            {
                CommentId = 502,
                TaskId = 100,
                UserId = 2,
                Content = "u2 content"
            };

            _context.Users.AddRange(user1, user2);
            _context.Projects.Add(project);
            _context.Tasks.Add(task);
            _context.Comments.AddRange(commentOwnedByUser1, commentOwnedByUser2);

            _context.SaveChanges();
        }

        // =====================================================
        // ADD COMMENT
        // =====================================================

        [Fact]
        public async System.Threading.Tasks.Task AddComment_PermissionDenied_RedirectsWithError()
        {
            // Arrange
            _controller.HttpContext.Session.SetInt32("RoleId", 1); // not intern lead

            // Act
            var result = await _controller.AddComment(100, "hello");

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("BacklogUI", redirect.ActionName);
            Assert.Equal("Backlog", redirect.ControllerName);
            Assert.Equal("You don't have permission to add comment.", _controller.TempData["Error"]);
        }

        [Fact]
        public async System.Threading.Tasks.Task AddComment_EmptyContent_RedirectsWithError()
        {
            // Arrange
            _controller.HttpContext.Session.SetInt32("RoleId", 2);
            _controller.HttpContext.Session.SetInt32("UserId", 1);

            // Act
            var result = await _controller.AddComment(100, "   ");

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("BacklogUI", redirect.ActionName);
            Assert.Equal("Backlog", redirect.ControllerName);
            Assert.Equal("Comment cannot be empty.", _controller.TempData["Error"]);
        }

        [Fact]
        public async System.Threading.Tasks.Task AddComment_Success_RedirectsToBacklog_AddsHistory_AndLogsActivity()
        {
            // Arrange
            _controller.HttpContext.Session.SetInt32("RoleId", 2);
            _controller.HttpContext.Session.SetInt32("UserId", 1);

            _commentServiceMock
                .Setup(s => s.AddCommentAsync(100, 1, "New comment"))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.AddComment(100, "New comment");

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("BacklogUI", redirect.ActionName);
            Assert.Equal("Backlog", redirect.ControllerName);
            Assert.Equal(1, redirect.RouteValues["projectId"]);

            Assert.Equal("Comment added successfully!", _controller.TempData["Success"]);

            _commentServiceMock.Verify(s => s.AddCommentAsync(100, 1, "New comment"), Times.Once);
            _taskHistoryServiceMock.Verify(h => h.AddAsync(100, 1, "COMMENT_ADDED", It.IsAny<string>()), Times.Once);
            _activityLogServiceMock.Verify(l => l.Log(
                1, 1, 100,
                "COMMENT_ADDED",
                It.IsAny<string>(),
                It.IsAny<DateTime>(),
                null
            ), Times.Once);
        }

        [Fact]
        public async System.Threading.Tasks.Task AddComment_Failed_ReturnsErrorAndRedirects()
        {
            // Arrange
            _commentServiceMock
                .Setup(s => s.AddCommentAsync(100, 1, It.IsAny<string>()))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.AddComment(100, "fail");

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("BacklogUI", redirect.ActionName);
            Assert.Equal("Failed to add comment.", _controller.TempData["Error"]);

            _taskHistoryServiceMock.Verify(h => h.AddAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _activityLogServiceMock.Verify(l => l.Log(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<DateTime>(), It.IsAny<int?>()
            ), Times.Never);
        }

        // =====================================================
        // GET COMMENTS
        // =====================================================

        [Fact]
        public async System.Threading.Tasks.Task GetComments_ReturnsJson()
        {
            // Arrange
            _commentServiceMock
                .Setup(s => s.GetCommentsByTaskIdAsync(100))
                .ReturnsAsync(new List<CommentDisplayViewModel>
                {
            new CommentDisplayViewModel
            {
                CommentId = 1,
                TaskId = 100,
                Content = "c1"
                // add other required fields nếu model bắt buộc
            }
                });

            // Act
            var result = await _controller.GetComments(100);

            // Assert
            var json = Assert.IsType<JsonResult>(result);
            var data = Assert.IsAssignableFrom<IEnumerable<CommentDisplayViewModel>>(json.Value);

            Assert.Single(data);
        }


        // =====================================================
        // UPDATE COMMENT
        // =====================================================

        [Fact]
        public async System.Threading.Tasks.Task UpdateComment_CommentNotFound_ReturnsNotFound()
        {
            // Arrange
            var model = new UpdateCommentVM { CommentId = 9999, Content = "x" };

            // Act
            var result = await _controller.UpdateComment(model);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async System.Threading.Tasks.Task UpdateComment_NotOwner_ReturnsForbid()
        {
            // Arrange: current user = 1, comment owned by user 2 (seeded: 502)
            _controller.HttpContext.Session.SetInt32("UserId", 1);

            var model = new UpdateCommentVM { CommentId = 502, Content = "new" };

            // Act
            var result = await _controller.UpdateComment(model);

            // Assert
            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async System.Threading.Tasks.Task UpdateComment_Success_UpdatesContent_ReturnsJson_AddsHistory_AndLogsActivity()
        {
            // Arrange: comment 501 owned by user 1
            _controller.HttpContext.Session.SetInt32("UserId", 1);

            var model = new UpdateCommentVM { CommentId = 501, Content = "updated!" };

            // Act
            var result = await _controller.UpdateComment(model);

            // Assert
            var json = Assert.IsType<JsonResult>(result);
            var payload = json.Value!.ToString();
            Assert.Contains("success", payload);

            var comment = _context.Comments.First(c => c.CommentId == 501);
            Assert.Equal("updated!", comment.Content);

            _taskHistoryServiceMock.Verify(h => h.AddAsync(100, 1, "COMMENT_UPDATED", It.IsAny<string>()), Times.Once);

            _activityLogServiceMock.Verify(l => l.Log(
                1, 1, 100,
                "COMMENT_EDITED",
                It.IsAny<string>(),
                It.IsAny<DateTime>(),
                null
            ), Times.Once);
        }

        // =====================================================
        // DELETE COMMENT
        // =====================================================

        [Fact]
        public async System.Threading.Tasks.Task DeleteComment_NotOwner_ReturnsForbid()
        {
            // Arrange: user 1 trying delete comment owned by user 2 (502)
            _controller.HttpContext.Session.SetInt32("UserId", 1);

            var req = new DeleteCommentRequest { CommentId = 502 };

            // Act
            var result = await _controller.DeleteComment(req);

            // Assert
            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async System.Threading.Tasks.Task DeleteComment_Success_RemovesComment_ReturnsOk_AddsHistory_AndLogsActivity()
        {
            // Arrange: user 1 delete own comment 501
            _controller.HttpContext.Session.SetInt32("UserId", 1);

            var req = new DeleteCommentRequest { CommentId = 501 };

            // Act
            var result = await _controller.DeleteComment(req);

            // Assert
            Assert.IsType<OkResult>(result);
            Assert.Null(_context.Comments.FirstOrDefault(c => c.CommentId == 501));

            _taskHistoryServiceMock.Verify(h => h.AddAsync(100, 1, "COMMENT_DELETED", It.IsAny<string>()), Times.Once);

            _activityLogServiceMock.Verify(l => l.Log(
                1, 1, 100,
                "COMMENT_DELETED",
                It.IsAny<string>(),
                It.IsAny<DateTime>(),
                null
            ), Times.Once);
        }

        [Fact]
        public async System.Threading.Tasks.Task DeleteComment_CommentMissing_ReturnsForbid()
        {
            // Arrange
            _controller.HttpContext.Session.SetInt32("UserId", 1);
            var req = new DeleteCommentRequest { CommentId = 99999 };

            // Act
            var result = await _controller.DeleteComment(req);

            // Assert: controller returns Forbid when comment == null
            Assert.IsType<ForbidResult>(result);
        }
    }
}