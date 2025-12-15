using Microsoft.AspNetCore.Mvc;
using Moq;
using PorjectManagement.Controllers;
using PorjectManagement.Models;
using PorjectManagement.Repository;
using PorjectManagement.Service;
using PorjectManagement.Service.Interface;
using PorjectManagement.Testing.IntergrationTest;
using Xunit;

namespace ProjectManagement.Testing.IntegrationTest
{
    public class UserLoginIntegrationTest
    {
        [Fact]
        public void Login_WithValidAccount_RedirectToHome()
        {
            // Arrange
            var context = TestDbContextFactory.Create();
            context.Users.Add(new User
            {
                UserId = 1,
                FullName = "Test User",
                Email = "test@gmail.com",
                PasswordHash = "12345",
                RoleId = 1,
                Status = UserStatus.Active
            });
            context.SaveChanges();
            var emailService = new Mock<IEmailService>().Object;
            var repo = new UserRepo(context);
            var service = new UserServices(repo);
            var controller = new UserController(service, emailService);

            // 🔴 GÁN HttpContext + Session
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    Session = new FakeSession()
                }
            };

            // Act
            var result = controller.Login("test@gmail.com", "12345");

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
        }


        [Fact]
        public void Login_WithWrongPassword_ReturnViewWithError()
        {
            var context = TestDbContextFactory.Create();
            context.Users.Add(new User
            {
                FullName = "User",
                Email = "user@gmail.com",
                PasswordHash = "12345",
                RoleId = 1,
                Status = UserStatus.Active
            });
            context.SaveChanges();
            var emailService = new Mock<IEmailService>().Object;
            var repo = new UserRepo(context);
            var service = new UserServices(repo);
            var controller = new UserController(service, emailService);

            var result = controller.Login("user@gmail.com", "wrong");

            var view = Assert.IsType<ViewResult>(result);
            Assert.NotNull(view);
        }

        //[Fact]
        //public void Register_NewUser_Success()
        //{
        //    var context = TestDbContextFactory.Create();
        //    var repo = new UserRepo(context);
        //    var emailService = new Mock<IEmailService>().Object;
        //    var service = new UserServices(repo);
        //    var controller = new UserController(service, emailService);

        //    var result = controller.Register(
        //        "New User",
        //        "new@gmail.com",
        //        "12345",
        //        "12345"
        //    );

        //    var redirect = Assert.IsType<RedirectToActionResult>(result);
        //    Assert.Equal("Login", redirect.ActionName);
        //    Assert.Equal(1, context.Users.Count());
        //}

        //[Fact]
        //public void ResetPasswordConfirm_UpdatePassword_Success()
        //{
        //    var context = TestDbContextFactory.Create();
        //    context.Users.Add(new User
        //    {
        //        FullName = "Reset User",
        //        Email = "reset@gmail.com",
        //        PasswordHash = "12345",
        //        RoleId = 2,
        //        Status = UserStatus.Active
        //    });
        //    context.SaveChanges();
        //    var emailService = new Mock<IEmailService>().Object;
        //    var repo = new UserRepo(context);
        //    var service = new UserServices(repo);
        //    var controller = new UserController(service, emailService);

        //    var result = controller.ResetPasswordConfirm(
        //        "reset@gmail.com",
        //        "67890",
        //        "67890"
        //    );

        //    var redirect = Assert.IsType<RedirectToActionResult>(result);
        //    Assert.Equal("Login", redirect.ActionName);

        //    var user = context.Users.First(u => u.Email == "reset@gmail.com");
        //    Assert.Equal("67890", user.PasswordHash);
        //}

    }
}
