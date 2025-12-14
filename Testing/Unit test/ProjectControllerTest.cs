using Microsoft.AspNetCore.Mvc;
using Moq;
using PorjectManagement.Service.Interface;
using Xunit;
using PorjectManagement.Models;

namespace PorjectManagement.Testing.Unit_test
{
    public class ProjectControllerTest
    {
        [Fact]
        public async System.Threading.Tasks.Task Index_ValidSession_ReturnsViewWithProjects()
        {
            // Arrange
            var userId = 1;
            var roleId = 2;

            var projects = new List<Project>
    {
        new Project { ProjectId = 1, ProjectName = "Lab Project" }
    };

            var mockService = new Mock<IProjectServices>();
            mockService
                .Setup(s => s.GetProjectsOfUserAsync(userId))
                .ReturnsAsync(projects);

            var controller = CreateControllerWithSession(userId, roleId, mockService);

            // Act
            var result = await controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Project>>(viewResult.Model);

            Assert.Single(model);
            Assert.Equal(roleId, controller.ViewBag.RoleId);
        }

    }
}
