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

        public ProjectControllerTests()
        {
            _mockService = new Mock<IProjectServices>();
        }

        private ProjectController CreateController(int? userId, int? roleId)
        {
            var controller = new ProjectController(_mockService.Object);
            ControllerTestHelper.CreateControllerWithSession(controller, userId, roleId);
            return controller;
        }

        /*// 1️⃣ UserId = 0 → Redirect Login
        [Fact]
        public async System.Threading.Tasks.Task Index_UserIdIsZero_RedirectsToLogin()
        {
            // Arrange
            var controller = CreateController(null, null);

            // Act
            var result = await controller.Index();

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirect.ActionName);
            Assert.Equal("User", redirect.ControllerName);
        }

        // 2️⃣ Session hợp lệ → Return View
        [Fact]
        public async System.Threading.Tasks.Task Index_ValidSession_ReturnsViewWithModel()
        {
            // Arrange
            var userId = 1;
            var roleId = 2;

            var projects = new List<Project>
        {
            new Project
            {
                ProjectId = 10,
                ProjectName = "Lab Project",
                Status = ProjectStatus.InProgress,
                UserProjects = new List<UserProject>
                {
                    new UserProject
                    {
                        IsLeader = true,
                        User = new User { FullName = "Intern Lead" }
                    },
                    new UserProject
                    {
                        IsLeader = false,
                        User = new User { FullName = "Member" }
                    }
                }
            }
        };

            _mockService
                .Setup(s => s.GetProjectsOfUserAsync(userId))
                .ReturnsAsync(projects);

            var controller = CreateController(userId, roleId);

            // Act
            var result = await controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);

            var model = Assert.IsAssignableFrom<List<ProjectListVM>>(viewResult.Model);
            Assert.Single(model);

            Assert.Equal("Lab Project", model[0].ProjectName);
            Assert.Equal("Intern Lead", model[0].LeaderName);
            Assert.Equal(2, model[0].MemberCount);

            Assert.Equal(roleId, controller.ViewBag.RoleId);
            Assert.NotNull(controller.ViewBag.Projects);
        }

        // 3️⃣ Service được gọi đúng userId
        [Fact]
        public async System.Threading.Tasks.Task Index_ValidSession_CallsServiceOnce()
        {
            // Arrange
            var userId = 5;
            var roleId = 1;

            _mockService
                .Setup(s => s.GetProjectsOfUserAsync(userId))
                .ReturnsAsync(new List<Project>());

            var controller = CreateController(userId, roleId);

            // Act
            await controller.Index();

            // Assert
            _mockService.Verify(
                s => s.GetProjectsOfUserAsync(userId),
                Times.Once
            );
        }*/
    }
}


