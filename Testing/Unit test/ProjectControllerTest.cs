using Microsoft.AspNetCore.Mvc;
using Moq;
using PorjectManagement.Controllers;
using PorjectManagement.Models;
using PorjectManagement.Service.Interface;
using PorjectManagement.Testing.Helpers;
using PorjectManagement.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace PorjectManagement.Testing.Unit_test
{
    public class ProjectControllerTests
    {
        private readonly Mock<IProjectServices> _mockService;
        private readonly ProjectController _controller;

        public ProjectControllerTests()
        {
            _mockService = new Mock<IProjectServices>();
            _controller = new ProjectController(_mockService.Object);

            var httpContext = new DefaultHttpContext();
            httpContext.Session = new TestSession();

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }
        [Fact]
        public async System.Threading.Tasks.Task Index_UserNotLoggedIn_RedirectToLogin()
        {
            // Arrange
            _controller.HttpContext.Session.SetInt32("UserId", 0);

            // Act
            var result = await _controller.Index(null, null);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirect.ActionName);
            Assert.Equal("User", redirect.ControllerName);
        }
        [Fact]
        public async System.Threading.Tasks.Task Index_UserLoggedIn_ReturnsViewWithProjects()
        {
            // Arrange
            _controller.HttpContext.Session.SetInt32("UserId", 1);
            _controller.HttpContext.Session.SetInt32("RoleId", 1);

            var projects = new List<Project>
    {
        new Project
        {
            ProjectId = 1,
            ProjectName = "Lab Management",
            Deadline = DateTime.Now.AddDays(10),
            Status = ProjectStatus.InProgress,
            UserProjects = new List<UserProject>
            {
                new UserProject
                {
                    IsLeader = true,
                    User = new User { FullName = "Mentor A" }
                }
            }
        }
    };

            _mockService
                .Setup(s => s.GetProjectsOfUserAsync(1))
                .ReturnsAsync(projects);

            // Act
            var result = await _controller.Index(null, null);

            // Assert
            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectFilterVM>(view.Model);

            Assert.Single(model.Projects);
            Assert.Equal("Lab Management", model.Projects.First().ProjectName);
        }
        [Fact]
        public async System.Threading.Tasks.Task Index_WithKeyword_FiltersProjectsCorrectly()
        {
            // Arrange
            _controller.HttpContext.Session.SetInt32("UserId", 1);

            var projects = new List<Project>
    {
        new Project
        {
            ProjectId = 1,
            ProjectName = "Lab Management",
            Status = ProjectStatus.InProgress,
            UserProjects = new List<UserProject>()
        },
        new Project
        {
            ProjectId = 2,
            ProjectName = "Library System",
            Status = ProjectStatus.InProgress,
            UserProjects = new List<UserProject>()
        }
    };

            _mockService
                .Setup(s => s.GetProjectsOfUserAsync(1))
                .ReturnsAsync(projects);

            // Act
            var result = await _controller.Index("Lab", null);

            // Assert
            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectFilterVM>(view.Model);

            Assert.Single(model.Projects);
            Assert.Equal("Lab Management", model.Projects.First().ProjectName);
        }
        [Fact]
        public async System.Threading.Tasks.Task Index_WithStatus_FiltersByStatus()
        {
            // Arrange
            _controller.HttpContext.Session.SetInt32("UserId", 1);

            var projects = new List<Project>
    {
        new Project
        {
            ProjectId = 1,
            ProjectName = "Project A",
            Status = ProjectStatus.Completed,
            UserProjects = new List<UserProject>()
        },
        new Project
        {
            ProjectId = 2,
            ProjectName = "Project B",
            Status = ProjectStatus.InProgress,
            UserProjects = new List<UserProject>()
        }
    };

            _mockService
                .Setup(s => s.GetProjectsOfUserAsync(1))
                .ReturnsAsync(projects);

            // Act
            var result = await _controller.Index(null, ProjectStatus.Completed);

            // Assert
            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectFilterVM>(view.Model);

            Assert.Single(model.Projects);
            Assert.Equal(ProjectStatus.Completed, model.Projects.First().Status);
        }

    }
}

