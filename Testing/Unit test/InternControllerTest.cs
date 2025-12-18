using Microsoft.AspNetCore.Mvc;
using Moq;
using PorjectManagement.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace PorjectManagement.Testing.Unit_test
{
    public class InternControllerTests
    {
        private readonly Mock<IInternService> _mockInternService;
        private readonly InternController _controller;

        public InternControllerTests()
        {
            _mockInternService = new Mock<IInternService>();
            _controller = new InternController(_mockInternService.Object);
        }

        /*[Fact]
        public async System.Threading.Tasks.Task Index_ReturnsViewResult_WithListOfInterns()
        {
            // Arrange
            var interns = new List<User>
        {
            new User { UserId = 1, FullName = "Intern A", RoleId = 2 },
            new User { UserId = 2, FullName = "Intern B", RoleId = 2 }
        };

            _mockInternService
                .Setup(s => s.GetInternsAsync())
                .ReturnsAsync(interns);

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<User>>(viewResult.Model);
            Assert.Equal(2, model.Count());
        }*/
    }

}
