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
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;
using Microsoft.AspNetCore.Mvc.Routing;


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

            var urlHelperMock = new Mock<IUrlHelper>();
            urlHelperMock
                .Setup(x => x.Action(It.IsAny<UrlActionContext>()))
                .Returns("mocked-url");

            var urlHelperFactoryMock = new Mock<IUrlHelperFactory>();
            urlHelperFactoryMock
                .Setup(f => f.GetUrlHelper(It.IsAny<ActionContext>()))
                .Returns(urlHelperMock.Object);


            httpContext.User = new ClaimsPrincipal(
                new ClaimsIdentity(
                    new[] { new Claim(ClaimTypes.NameIdentifier, "1") },
                    "TestAuth"
                )
            );

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

            // ✅ TempData
            _controller.TempData = new TempDataDictionary(
                httpContext,
                Mock.Of<ITempDataProvider>()
            );
        }

        [Fact]
        public async System.Threading.Tasks.Task AddMembers_Get_NoRole_RedirectsToHome()
        {
            // Arrange
            _controller.HttpContext.Session.SetInt32("RoleId", 1);

            // Act
            var result = await _controller.AddMembers(1);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("Home", redirect.ControllerName);
        }

        [Fact]
        public async System.Threading.Tasks.Task AddMembers_Get_NullId_ReturnsEmptyViewModel()
        {
            // Arrange
            _userProjectServiceMock
                .Setup(x => x.GetAllProjectsAsync())
                .ReturnsAsync(new List<Project>());

            // Act
            var result = await _controller.AddMembers((null));

            // Assert
            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<AddMembersViewModel>(view.Model);

            Assert.Equal(0, model.ProjectId);
            Assert.Equal("Chưa chọn", model.ProjectName);
            Assert.NotNull(model.AllProjects);
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
        public async System.Threading.Tasks.Task AddMembers_Get_ValidProject_ReturnsViewWithUsers()
        {
            // Arrange
            _userProjectServiceMock
                .Setup(x => x.GetAllProjectsAsync())
                .ReturnsAsync(new List<Project>());

            _userProjectServiceMock
                .Setup(x => x.GetProjectByIdAsync(1))
                .ReturnsAsync(new Project
                {
                    ProjectId = 1,
                    ProjectName = "Test Project"
                });

            _userProjectServiceMock
                .Setup(x => x.GetAllUsersAsync())
                .ReturnsAsync(new List<User>
                {
                    new User
                    {
                        UserId = 1,
                        FullName = "Test User",
                        Email = "test@mail.com",
                        CreatedAt = DateTime.Now
                    }
                });

            // Act
            var result = await _controller.AddMembers(1);

            // Assert
            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<AddMembersViewModel>(view.Model);

            Assert.Equal(1, model.ProjectId);
            Assert.Equal("Test Project", model.ProjectName);
            Assert.Single(model.Users);
        }

        [Fact]
        public async System.Threading.Tasks.Task AddMembers_Post_InvalidRole_RedirectsHome()
        {
            // Arrange
            _controller.HttpContext.Session.SetInt32("RoleId", 1);

            // Act
            var result = await _controller.AddMembers(new AddMembersViewModel());

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("Home", redirect.ControllerName);
        }

        [Fact]
        public async System.Threading.Tasks.Task AddMembers_Post_InvalidModel_ReturnsView()
        {
            // Arrange
            var model = new AddMembersViewModel
            {
                ProjectId = 0 // invalid
            };

            _userProjectServiceMock
                .Setup(x => x.GetAllProjectsAsync())
                .ReturnsAsync(new List<Project>());

            _userProjectServiceMock
                .Setup(x => x.GetAllUsersAsync())
                .ReturnsAsync(new List<User>());

            // Act
            var result = await _controller.AddMembers(model);

            // Assert
            var view = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
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
        }

        [Fact]
        public async System.Threading.Tasks.Task AddMembers_Post_ValidUsers_RedirectsAndCallsService()
        {
            // Arrange
            var model = new AddMembersViewModel
            {
                ProjectId = 1,
                SelectedUserIds = new List<int> { 1, 2 }
            };

            _userProjectServiceMock
                .Setup(x => x.AddUsersToProjectAsync(1, model.SelectedUserIds))
                .ReturnsAsync(new Dictionary<int, string>());

            // Act
            var result = await _controller.AddMembers(model);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("AddMembers", redirect.ActionName);

            _userProjectServiceMock.Verify(
                x => x.AddUsersToProjectAsync(1, model.SelectedUserIds),
                Times.Once
            );
        }
    }
}
