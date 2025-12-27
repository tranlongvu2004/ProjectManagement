using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using PorjectManagement.Controllers;
using PorjectManagement.Models;
using PorjectManagement.Service.Interface;
using System;
using System.Linq;
using Xunit;

namespace PorjectManagement.Testing.UnitTest
{
    public class UserControllerTests
    {
        private readonly Mock<IUserServices> _mockService;
        private readonly Mock<IEmailService> _mockEmail;
        private readonly UserController _controller;
        private readonly DefaultHttpContext _context;

        public UserControllerTests()
        {
            _mockService = new Mock<IUserServices>();
            _mockEmail = new Mock<IEmailService>();
            _controller = new UserController(_mockService.Object, _mockEmail.Object);

            _context = new DefaultHttpContext();
            _context.Session = new TestSession(); // dùng TestSession của fen

            // ✅ Mock Url.Action để tránh null khi build link
            var urlHelperMock = new Mock<IUrlHelper>();
            urlHelperMock
                .Setup(x => x.Action(It.IsAny<UrlActionContext>()))
                .Returns("mocked-url");

            _controller.Url = urlHelperMock.Object;

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = _context
            };

            _controller.TempData = new TempDataDictionary(_context, Mock.Of<ITempDataProvider>());
        }

        // ============================================================
        // LOGIN
        // ============================================================

        //[Fact]
        //public void Login_MissingEmailOrPassword_ReturnView()
        //{
        //    var result = _controller.Login("", "");
        //    Assert.IsType<ViewResult>(result);
        //    Assert.Equal("Please enter email and password", _controller.ViewBag.Error);
        //}

        [Fact]
        public void Login_InvalidCredentials_ReturnView()
        {
            _mockService.Setup(s => s.IsLoginValid("a@gmail.com", "123")).Returns(false);

            var result = _controller.Login("a@gmail.com", "123");

            Assert.IsType<ViewResult>(result);
            Assert.Equal("Email or password is incorrect.", _controller.ViewBag.Error);
        }

        [Fact]
        public void Login_ValidCredentials_UserNull_ReturnView()
        {
            _mockService.Setup(s => s.IsLoginValid("a@gmail.com", "123")).Returns(true);
            _mockService.Setup(s => s.GetUser("a@gmail.com")).Returns((User?)null);

            var result = _controller.Login("a@gmail.com", "123");

            Assert.IsType<ViewResult>(result);
            Assert.Equal("Account not found.", _controller.ViewBag.Error);
        }

        [Fact]
        public void Login_Role3_RedirectToProject_AndSetSession()
        {
            _mockService.Setup(s => s.IsLoginValid("test@example.com", "123")).Returns(true);
            _mockService.Setup(s => s.GetUser("test@example.com")).Returns(
                new User { UserId = 5, Email = "test@example.com", RoleId = 3, FullName = "Test User" });

            var result = _controller.Login("test@example.com", "123");

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("Project", redirect.ControllerName);

            Assert.Equal("test@example.com", _context.Session.GetString("UserEmail"));
            Assert.Equal(3, _context.Session.GetInt32("RoleId"));
            Assert.Equal(5, _context.Session.GetInt32("UserId"));
        }

        [Fact]
        public void Login_Role1_RedirectHome()
        {
            _mockService.Setup(s => s.IsLoginValid("a", "b")).Returns(true);
            _mockService.Setup(s => s.GetUser("a")).Returns(
                new User { UserId = 1, Email = "a", RoleId = 1, FullName = "A" });

            var result = _controller.Login("a", "b");

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("Home", redirect.ControllerName);
        }

        // ============================================================
        // REGISTER
        // ============================================================

        [Fact]
        public void Register_InvalidEmail_ReturnView()
        {
            var result = _controller.Register("");

            Assert.IsType<ViewResult>(result);
            Assert.Equal("Email is required", _controller.ViewBag.Error);
        }

        [Fact]
        public void Register_EmailExists_ReturnView()
        {
            _mockService.Setup(s => s.GetUser("a@gmail.com"))
                        .Returns(new User());

            var result = _controller.Register("a@gmail.com");

            Assert.IsType<ViewResult>(result);
            Assert.Equal("Email already exists", _controller.ViewBag.Error);
        }

        [Fact]
        public void Register_Valid_ReturnCheckEmailView_AndSendEmail()
        {
            _mockService.Setup(s => s.GetUser("a@gmail.com"))
                        .Returns((User?)null);

            var result = _controller.Register("a@gmail.com");

            var view = Assert.IsType<ViewResult>(result);
            Assert.Equal("CheckEmail", view.ViewName);

            _mockService.Verify(s => s.CreateAccount(It.IsAny<User>()), Times.Once);
            _mockEmail.Verify(e => e.Send("a@gmail.com", It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        // ============================================================
        // COMPLETE REGISTER (POST)
        // ============================================================

        [Fact]
        public void CompleteRegister_PasswordMismatch_ReturnView()
        {
            _mockService.Setup(s => s.GetUser("a@gmail.com"))
                .Returns(new User { Email = "a@gmail.com", Status = UserStatus.Inactive });

            var result = _controller.CompleteRegister("a@gmail.com", "User", "12345", "45655");

            Assert.IsType<ViewResult>(result);
            Assert.Equal("Password Mismatch", _controller.ViewBag.Error);
        }

        [Fact]
        public void CompleteRegister_Valid_RedirectToLogin()
        {
            var user = new User { Email = "a@gmail.com", Status = UserStatus.Inactive };
            _mockService.Setup(s => s.GetUser("a@gmail.com")).Returns(user);

            var result = _controller.CompleteRegister("a@gmail.com", "User", "12345", "12345");

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirect.ActionName);

            Assert.Equal(UserStatus.Active, user.Status);
            _mockService.Verify(s => s.UpdateUser(user), Times.Once);
        }

        // ============================================================
        // RESET PASSWORD
        // ============================================================

        [Fact]
        public void ResetPassword_EmailEmpty_ReturnView()
        {
            var result = _controller.ResetPassword("");

            Assert.IsType<ViewResult>(result);
            Assert.Equal("Please enter email.", _controller.ViewBag.Error);
        }

        [Fact]
        public void ResetPassword_EmailNotFound_ReturnView()
        {
            _mockService.Setup(s => s.GetUser("x@gmail.com")).Returns((User?)null);

            var result = _controller.ResetPassword("x@gmail.com");

            Assert.IsType<ViewResult>(result);
            Assert.Equal("Email does not exist.", _controller.ViewBag.Error);
        }

        [Fact]
        public void ResetPassword_EmailFound_ReturnView_AndSendEmail()
        {
            _mockService.Setup(s => s.GetUser("a@gmail.com"))
                .Returns(new User { Email = "a@gmail.com" });

            var result = _controller.ResetPassword("a@gmail.com");

            Assert.IsType<ViewResult>(result);
            _mockEmail.Verify(e => e.Send("a@gmail.com", It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        // ============================================================
        // PROFILE + LOGOUT
        // ============================================================

        [Fact]
        public void Profile_NoSession_RedirectToLogin()
        {
            _context.Session.Clear();

            var result = _controller.Profile();

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirect.ActionName);
        }

        [Fact]
        public void Profile_HasSession_ReturnView()
        {
            _context.Session.SetInt32("UserId", 10);

            _mockService.Setup(s => s.GetUserById(10))
                .Returns(new User { UserId = 10, FullName = "Test" });

            var result = _controller.Profile();

            var view = Assert.IsType<ViewResult>(result);
            Assert.IsType<User>(view.Model);
        }

        [Fact]
        public void Logout_ClearSessionAndRedirect()
        {
            _context.Session.SetString("UserEmail", "a@gmail.com");

            var result = _controller.Logout();

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirect.ActionName);
            Assert.False(_context.Session.Keys.Any());
        }
    }
}
