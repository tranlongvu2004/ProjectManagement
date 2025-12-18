using Microsoft.AspNetCore.Mvc;
using Moq;
using PorjectManagement.Models;
using PorjectManagement.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace PorjectManagement.Testing.Unit_test
{

    public class InternControllerTests
    {
        private readonly Mock<IInternService> _mockService;
        private readonly InternController _controller;

        public InternControllerTests()
        {
            _mockService = new Mock<IInternService>();
            _controller = new InternController(_mockService.Object);

            // Fake HttpContext + Session
            var httpContext = new DefaultHttpContext();
            httpContext.Session = new TestSession();
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }

        [Fact]
        public async System.Threading.Tasks.Task Index_RoleIsNotMentor_RedirectToAccessDeny()
        {
            // Arrange
            _controller.HttpContext.Session.SetInt32("RoleId", 2);

            // Act
            var result = await _controller.Index(null);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("AccessDeny", redirect.ActionName);
            Assert.Equal("Error", redirect.ControllerName);
        }
        [Fact]
        public async System.Threading.Tasks.Task Index_RoleIsMentor_ReturnsViewWithInterns()
        {
            // Arrange
            _controller.HttpContext.Session.SetInt32("RoleId", 1);

            var interns = new List<User>
    {
        new User { UserId = 1, FullName = "Nguyen Van A", Email = "a@test.com", CreatedAt = DateTime.Now },
        new User { UserId = 2, FullName = "Tran Thi B", Email = "b@test.com", CreatedAt = DateTime.Now }
    }.AsQueryable();

            _mockService
                .Setup(s => s.GetInternsAsync())
                .ReturnsAsync(interns);

            // Act
            var result = await _controller.Index(null);

            // Assert
            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<InternFilterVM>(view.Model);

            Assert.Equal(2, model.Interns.Count);
            Assert.Equal(1, model.Page);
        }
        [Fact]
        public async System.Threading.Tasks.Task Index_WithKeyword_FiltersInternsCorrectly()
        {
            // Arrange
            _controller.HttpContext.Session.SetInt32("RoleId", 1);

            var interns = new List<User>
    {
        new User { UserId = 1, FullName = "Nguyen Van A", Email = "a@test.com", CreatedAt = DateTime.Now },
        new User { UserId = 2, FullName = "Tran Thi B", Email = "b@test.com", CreatedAt = DateTime.Now }
    }.AsQueryable();

            _mockService
                .Setup(s => s.GetInternsAsync())
                .ReturnsAsync(interns);

            // Act
            var result = await _controller.Index("Nguyen");

            // Assert
            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<InternFilterVM>(view.Model);

            Assert.Single(model.Interns);
            Assert.Equal("Nguyen Van A", model.Interns.First().FullName);
        }
    }
    }
