using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using Moq;
using NuGet.ContentModel;
using PorjectManagement.Controllers;
using PorjectManagement.Models;
using PorjectManagement.Service.Interface;
using System;
using System.Collections.Generic;

namespace PorjectManagement.Testing.UnitTest
{
    public class MockHttpSession : ISession
    {
        Dictionary<string, byte[]> _store = new();

        public IEnumerable<string> Keys => _store.Keys;

        public string Id => Guid.NewGuid().ToString();

        public bool IsAvailable => true;

        public void Clear() => _store.Clear();

        public void Remove(string key) => _store.Remove(key);

        public void Set(string key, byte[] value) => _store[key] = value;

        public bool TryGetValue(string key, out byte[] value)
            => _store.TryGetValue(key, out value);
        public System.Threading.Tasks.Task LoadAsync(CancellationToken cancellationToken = default)
     => System.Threading.Tasks.Task.CompletedTask;

        public System.Threading.Tasks.Task CommitAsync(CancellationToken cancellationToken = default)
            => System.Threading.Tasks.Task.CompletedTask;

    }

    [TestClass]
    public class UserControllerTests
    {
        private Mock<IUserServices> _mockService = null!;
        private Mock<IEmailService> _mockEmail = null!;
        private UserController _controller = null!;
        private DefaultHttpContext _context = null!;

        [TestInitialize]
        public void Setup()
        {
            _mockService = new Mock<IUserServices>();
            _mockEmail = new Mock<IEmailService>();
            _controller = new UserController(_mockService.Object, _mockEmail.Object);

            _context = new DefaultHttpContext();
            _context.Session = new MockHttpSession();

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = _context
            };

            _controller.TempData = new TempDataDictionary(_context, Mock.Of<ITempDataProvider>());
        }

        // ============================================================
        // LOGIN TESTS
        // ============================================================

        [TestMethod]
        public void Login_MissingEmailOrPassword_ReturnView()
        {
            var result = _controller.Login("", "");
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            Assert.AreEqual("Vui lòng nhập email và password", _controller.ViewBag.Error);
        }

        [TestMethod]
        public void Login_InvalidCredentials_ReturnView()
        {
            _mockService.Setup(s => s.IsLoginValid("a@gmail.com", "123")).Returns(false);

            var result = _controller.Login("a@gmail.com", "123");

            Assert.IsInstanceOfType(result, typeof(ViewResult));
            Assert.AreEqual("Email hoặc mật khẩu không đúng.", _controller.ViewBag.Error);
        }

        [TestMethod]
        public void Login_ValidCredentials_UserNull_ReturnView()
        {
            _mockService.Setup(s => s.IsLoginValid("a@gmail.com", "123")).Returns(true);
            _mockService.Setup(s => s.GetUser("a@gmail.com")).Returns((User?)null);

            var result = _controller.Login("a@gmail.com", "123");

            Assert.IsInstanceOfType(result, typeof(ViewResult));
            Assert.AreEqual("Không tìm thấy tài khoản.", _controller.ViewBag.Error);
        }

        [TestMethod]
        public void Login_Role3_RedirectToProject()
        {
            _mockService.Setup(s => s.IsLoginValid("test@example.com", "123")).Returns(true);
            _mockService.Setup(s => s.GetUser("test@example.com")).Returns(
                new User
                {
                    UserId = 5,
                    Email = "test@example.com",
                    RoleId = 3
                });

            var result = _controller.Login("test@example.com", "123") as RedirectToActionResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.ActionName);
            Assert.AreEqual("Project", result.ControllerName);

            Assert.AreEqual("test@example.com", _context.Session.GetString("UserEmail"));
            Assert.AreEqual(3, _context.Session.GetInt32("RoleId"));
        }

        [TestMethod]
        public void Login_Role1_Redirect()
        {
            _mockService.Setup(s => s.IsLoginValid("a", "b")).Returns(true);
            _mockService.Setup(s => s.GetUser("a")).Returns(new User
            {
                UserId = 1,
                Email = "a",
                RoleId = 1
            });

            var result = _controller.Login("a", "b") as RedirectToActionResult;

            Assert.AreEqual("Home", result!.ControllerName);
        }

        // ============================================================
        // REGISTER TESTS
        // ============================================================

        [TestMethod]
        public void Register_InvalidEmail_ReturnView()
        {
            var result = _controller.Register("");

            Assert.IsInstanceOfType(result, typeof(ViewResult));
            Assert.AreEqual("Email is required.", _controller.ViewBag.Error);
        }

        [TestMethod]
        public void Register_EmailExists_ReturnView()
        {
            _mockService.Setup(s => s.GetUser("a@gmail.com"))
                        .Returns(new User());

            var result = _controller.Register("a@gmail.com");

            Assert.IsInstanceOfType(result, typeof(ViewResult));
            Assert.AreEqual("Email already exists.", _controller.ViewBag.Error);
        }

        [TestMethod]
        public void Register_Valid_ReturnCheckEmailView()
        {
            _mockService.Setup(s => s.GetUser("a@gmail.com"))
                        .Returns((User?)null);

            var result = _controller.Register("a@gmail.com") as ViewResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("CheckEmail", result.ViewName);

            _mockService.Verify(s => s.CreateAccount(It.IsAny<User>()), Times.Once);
        }


        //=============================================================
        // Complete Registration Tests
        //=============================================================
        [TestMethod]
        public void CompleteRegister_PasswordMismatch_ReturnView()
        {
            _mockService.Setup(s => s.GetUser("a@gmail.com"))
                .Returns(new User { Email = "a@gmail.com", Status = UserStatus.Inactive });

            var result = _controller.CompleteRegister(
                "a@gmail.com", "User", "12345", "45655");

            Assert.IsInstanceOfType(result, typeof(ViewResult));
            Assert.AreEqual("Password mismatch", _controller.ViewBag.Error);
        }


        [TestMethod]
        public void CompleteRegister_Valid_RedirectToLogin()
        {
            var user = new User
            {
                Email = "a@gmail.com",
                Status = UserStatus.Inactive
            };

            _mockService.Setup(s => s.GetUser("a@gmail.com"))
                .Returns(user);

            var result = _controller.CompleteRegister(
                "a@gmail.com", "User", "12345", "12345")
                as RedirectToActionResult;

            Assert.AreEqual("Login", result!.ActionName);
            Assert.AreEqual(UserStatus.Active, user.Status);

            _mockService.Verify(s => s.UpdateUser(user), Times.Once);
        }


        // ============================================================
        // RESET PASSWORD TESTS
        // ============================================================

        [TestMethod]
        public void ResetPassword_EmailEmpty_ReturnView()
        {
            var result = _controller.ResetPassword("");

            Assert.IsInstanceOfType(result, typeof(ViewResult));
            Assert.AreEqual("Vui lòng nhập email.", _controller.ViewBag.Error);
        }

        [TestMethod]
        public void ResetPassword_EmailNotFound_ReturnView()
        {
            _mockService.Setup(s => s.GetUser("x@gmail.com")).Returns((User?)null);

            var result = _controller.ResetPassword("x@gmail.com");

            Assert.AreEqual("Email không tồn tại.", _controller.ViewBag.Error);
        }

        [TestMethod]
        public void ResetPassword_EmailFound_Redirect()
        {
            _mockService.Setup(s => s.GetUser("a@gmail.com"))
                .Returns(new User());

            var result = _controller.ResetPassword("a@gmail.com")
                as RedirectToActionResult;

            Assert.AreEqual("ResetPasswordConfirm", result!.ActionName);
        }

        // ============================================================
        // RESET PASSWORD CONFIRM
        // ============================================================

        [TestMethod]
        public void ResetPasswordConfirm_MissingPassword_ReturnView()
        {
            var token = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("email" + "|" + DateTime.Now));
            var result = _controller.ResetPasswordConfirm(
                "email",
                token,
                "",
                ""
            ) as ViewResult;

            Assert.AreEqual("Password must have at least 5 characters.", _controller.ViewBag.Error);
        }


        [TestMethod]
        public void ResetPasswordConfirm_MismatchPassword_ReturnView()
        {
            var token = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("email" + "|" + DateTime.Now));

            var result = _controller.ResetPasswordConfirm(
                "email",
                token,
                "12345",
                "67890"
            ) as ViewResult;

            Assert.AreEqual("Password mismatch.", _controller.ViewBag.Error);
        }


        [TestMethod]
        public void ResetPasswordConfirm_UserNotFound_ReturnInvalidLink()
        {
            _mockService.Setup(s => s.GetUser("abc"))
                        .Returns((User?)null);
            var token = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("abc" + "|" + DateTime.Now));
            var result = _controller.ResetPasswordConfirm(
                "abc",
                token,
                "12345",
                "12345"
            );
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            Assert.AreEqual("InvalidLink", ((ViewResult)result).ViewName);
        }

        [TestMethod]
        public void ResetPasswordConfirm_Valid_Redirect()
        {
            _mockService.Setup(s => s.GetUser("intern1@example.com"))
                .Returns(new User
                {
                    Email = "intern1@example.com",
                    PasswordHash = "old"
                });

            var token = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("intern1@example.com" + "|" + DateTime.Now));

            var result = _controller.ResetPasswordConfirm(
                "intern1@example.com",
                token,
                "11111",
                "11111"
            ) as RedirectToActionResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("Login", result.ActionName);

            _mockService.Verify(
                s => s.UpdateUser(It.Is<User>(u => u.PasswordHash == "11111")),
                Times.Once
            );
        }


        // ============================================================
        // PROFILE
        // ============================================================

        [TestMethod]
        public void Profile_NoSession_RedirectToLogin()
        {
            _context.Session.Clear();

            var result = _controller.Profile() as RedirectToActionResult;

            Assert.AreEqual("Login", result!.ActionName);
        }

        [TestMethod]
        public void Profile_HasSession_ReturnView()
        {
            _context.Session.SetInt32("UserId", 10);

            _mockService.Setup(s => s.GetUserById(10))
                .Returns(new User { UserId = 10, FullName = "Test" });

            var result = _controller.Profile() as ViewResult;

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Model, typeof(User));
        }

        // ============================================================
        // LOGOUT
        // ============================================================

        [TestMethod]
        public void Logout_ClearSessionAndRedirect()
        {
            _context.Session.SetString("UserEmail", "a@gmail.com");

            var result = _controller.Logout() as RedirectToActionResult;

            Assert.AreEqual("Login", result!.ActionName);
            Assert.AreEqual(0, (_context.Session as MockHttpSession)!.Keys.Count());
        }
    }
}