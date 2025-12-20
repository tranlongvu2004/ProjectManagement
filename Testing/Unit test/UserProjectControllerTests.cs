using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using PorjectManagement.Controllers;
using PorjectManagement.Models;
using PorjectManagement.Service.Interface;
using PorjectManagement.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace PorjectManagement.Tests.Controllers
{
    public class UserProjectControllerTests
    {
        private readonly Mock<IUserProjectService> _userProjectServiceMock;
        private readonly UserProjectController _controller;

        public UserProjectControllerTests()
        {
            _userProjectServiceMock = new Mock<IUserProjectService>();
            _controller = new UserProjectController(_userProjectServiceMock.Object);

            var httpContext = new DefaultHttpContext();
            httpContext.Session = new TestSession();
            httpContext.Session.SetInt32("UserId", 1);
            httpContext.Session.SetInt32("RoleId", 2);

            httpContext.User = new ClaimsPrincipal(
                new ClaimsIdentity(
                    new[] { new Claim(ClaimTypes.NameIdentifier, "1") },
                    "TestAuth"
                )
            );

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            _controller.TempData = new TempDataDictionary(
                httpContext,
                Mock.Of<ITempDataProvider>()
            );
        }

        // =====================================================
        // GET AddMembers
        // =====================================================

        [Fact]
        public async System.Threading.Tasks.Task AddMembers_Get_InvalidRole_RedirectsToAccessDeny_Error()
        {
            // Arrange
            _controller.HttpContext.Session.SetInt32("RoleId", 1);

            // Act
            var result = await _controller.AddMembers(1);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("AccessDeny", redirect.ActionName);
            Assert.Equal("Error", redirect.ControllerName);
        }

        [Fact]
        public async System.Threading.Tasks.Task AddMembers_Get_ProjectNotFound_ReturnsNotFound()
        {
            // Arrange
            _userProjectServiceMock
                .Setup(x => x.GetProjectByIdAsync(1))
                .ReturnsAsync((Project?)null);

            // Act
            var result = await _controller.AddMembers(1);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async System.Threading.Tasks.Task AddMembers_Get_ValidProject_ReturnsViewWithFilteredUsers()
        {
            // Arrange
            _userProjectServiceMock
                .Setup(x => x.GetProjectByIdAsync(1))
                .ReturnsAsync(new Project
                {
                    ProjectId = 1,
                    ProjectName = "Test Project"
                });

            // Users already in project (NoMentor list)
            _userProjectServiceMock
                .Setup(x => x.GetUsersByProjectIdNoMentorAsync(1))
                .ReturnsAsync(new List<User>
                {
                    new User { UserId = 10, FullName = "Already In Project", Email = "a@a.com" }
                });

            // All users
            _userProjectServiceMock
                .Setup(x => x.GetAllUsersAsync())
                .ReturnsAsync(new List<User>
                {
                    // this one is already in project -> should be filtered out
                    new User { UserId = 10, FullName = "Already In Project", Email = "a@a.com", RoleId = 2, CreatedAt = DateTime.Now },

                    // this one has RoleId=1 -> should be filtered out by controller (u.RoleId != 1)
                    new User { UserId = 11, FullName = "Mentor", Email = "m@m.com", RoleId = 1, CreatedAt = DateTime.Now },

                    // this one is valid -> should remain
                    new User { UserId = 12, FullName = "Valid User", Email = "v@v.com", RoleId = 2, CreatedAt = DateTime.Now }
                });

            // Act
            var result = await _controller.AddMembers(1);

            // Assert
            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<AddMembersViewModel>(view.Model);

            Assert.Equal(1, model.ProjectId);
            Assert.Equal("Test Project", model.ProjectName);

            // Only "Valid User" remains
            Assert.Single(model.Users);
            Assert.Equal(12, model.Users[0].UserId);
            Assert.Equal("Valid User", model.Users[0].FullName);
        }

        // =====================================================
        // POST AddMembers
        // =====================================================

        [Fact]
        public async System.Threading.Tasks.Task AddMembers_Post_InvalidRole_RedirectsToAccessDeny_User()
        {
            // Arrange
            _controller.HttpContext.Session.SetInt32("RoleId", 1);

            // Act
            var result = await _controller.AddMembers(new AddMembersViewModel { ProjectId = 1 });

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("AccessDeny", redirect.ActionName);
            Assert.Equal("User", redirect.ControllerName);
        }

        [Fact]
        public async System.Threading.Tasks.Task AddMembers_Post_InvalidModel_ReturnsViewWithUsers()
        {
            // Arrange
            var model = new AddMembersViewModel
            {
                ProjectId = 0 // invalid -> controller adds ModelError
            };

            _userProjectServiceMock
                .Setup(x => x.GetUsersByProjectIdAsync(It.IsAny<int>()))
                .ReturnsAsync(new List<User>());

            _userProjectServiceMock
                .Setup(x => x.GetAllUsersAsync())
                .ReturnsAsync(new List<User>
                {
                    new User { UserId = 12, FullName = "Valid User", Email = "v@v.com", RoleId = 2, CreatedAt = DateTime.Now }
                });

            // Act
            var result = await _controller.AddMembers(model);

            // Assert
            var view = Assert.IsType<ViewResult>(result);
            var returnedModel = Assert.IsType<AddMembersViewModel>(view.Model);

            Assert.False(_controller.ModelState.IsValid);
            Assert.NotNull(returnedModel.Users);
            Assert.Single(returnedModel.Users);
        }

        [Fact]
        public async System.Threading.Tasks.Task AddMembers_Post_NoUsersSelected_RedirectsWithInfo()
        {
            // Arrange
            var model = new AddMembersViewModel
            {
                ProjectId = 1,
                SelectedUserIds = null
            };

            // Act
            var result = await _controller.AddMembers(model);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("AddMembers", redirect.ActionName);
            Assert.Null(_controller.TempData["AddResults"]);
            Assert.Equal("No member is selected.", _controller.TempData["Info"]);
        }

        [Fact]
        public async System.Threading.Tasks.Task AddMembers_Post_ValidUsers_RedirectsAndCallsService_SetsTempData()
        {
            // Arrange
            var model = new AddMembersViewModel
            {
                ProjectId = 1,
                SelectedUserIds = new List<int> { 1, 2 }
            };

            var serviceResult = new Dictionary<int, string>
            {
                { 1, "Added" },
                { 2, "Added" }
            };

            _userProjectServiceMock
                .Setup(x => x.AddUsersToProjectAsync(1, model.SelectedUserIds))
                .ReturnsAsync(serviceResult);

            // Act
            var result = await _controller.AddMembers(model);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("AddMembers", redirect.ActionName);

            _userProjectServiceMock.Verify(
                x => x.AddUsersToProjectAsync(1, model.SelectedUserIds),
                Times.Once
            );

            Assert.NotNull(_controller.TempData["AddResults"]);
            Assert.Equal("Add member to project successfully", _controller.TempData["Success"]);
        }
    }
}
