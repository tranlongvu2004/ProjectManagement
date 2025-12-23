

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PorjectManagement.Controllers;
using PorjectManagement.Models;
//using Testing;

namespace PorjectManagement.Testing
{
    public class ReportControllerTest
    {
        private Mock<IReportService> _mockService = null!;
        private ReportController _controller = null!;
        private DefaultHttpContext _context = null!;

        [TestInitialize]
        public void Setup()
        {
            _mockService = new Mock<IReportService>();
            _controller = new ReportController(_mockService.Object);

            _context = new DefaultHttpContext();
          //  _context.Session = new MockHttpSession();

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = _context
            };

            _controller.TempData = new TempDataDictionary(
                _context,
                Mock.Of<ITempDataProvider>()
            );
        }

        // ============================================================
        // VIEW REPORT TESTS
        // ============================================================

        [TestMethod]
        public void ViewReport_NoSession_ReturnUnauthorized()
        {
            // Act
            var result = _controller.ViewReport(1);

            // Assert
            Assert.IsInstanceOfType(result, typeof(UnauthorizedResult));
        }

        [TestMethod]
        public void ViewReport_NotLeader_ReturnForbid()
        {
            // Arrange
            _context.Session.SetInt32("UserId", 10);

            _mockService
                .Setup(s => s.IsLeaderOfProject(10, 1))
                .Returns(false);

            // Act
            var result = _controller.ViewReport(1);

            // Assert
            Assert.IsInstanceOfType(result, typeof(ForbidResult));
        }


        [TestMethod]
        public void ViewReport_IsLeader_ReturnViewWithDailyReports()
        {
            // Arrange
            _context.Session.SetInt32("UserId", 10);

            _mockService
                .Setup(s => s.IsLeaderOfProject(10, 1))
                .Returns(true);

            var fakeReports = new List<CreateReportViewModel>
    {
        new CreateReportViewModel
        {
            ProjectId = 1
        },
        new CreateReportViewModel
        {
            ProjectId = 1
        }
    };

            _mockService
                .Setup(s => s.GetReportsByProjectId(1))
                .Returns(fakeReports);

        }
    }
}
