using Moq;
using PorjectManagement.Models;
using PorjectManagement.Repository.Interface;
using PorjectManagement.Service;
using System;
using Xunit;

namespace Testing.UnitTest
{
    public class UserServicesTests
    {
        private readonly Mock<IUserRepo> _mockRepo;
        private readonly UserServices _service;

        public UserServicesTests()
        {
            _mockRepo = new Mock<IUserRepo>();
            _service = new UserServices(_mockRepo.Object);
        }

        [Fact]
        public void CreateAccount_ValidUser_ReturnUser()
        {
            var user = new User { Email = "a@gmail.com" };

            _mockRepo.Setup(r => r.CreateAccount(user))
                     .Returns(user);

            var result = _service.CreateAccount(user);

            Assert.NotNull(result);
            Assert.Equal("a@gmail.com", result!.Email);
            _mockRepo.Verify(r => r.CreateAccount(user), Times.Once);
        }

        [Fact]
        public void GetUser_EmailExists_ReturnUser()
        {
            _mockRepo.Setup(r => r.GetUserByEmail("test@gmail.com"))
                     .Returns(new User { Email = "test@gmail.com" });

            var result = _service.GetUser("test@gmail.com");

            Assert.NotNull(result);
            Assert.Equal("test@gmail.com", result!.Email);
        }

        [Fact]
        public void GetUserById_ValidId_ReturnUser()
        {
            _mockRepo.Setup(r => r.getUserById(1))
                     .Returns(new User { UserId = 1 });

            var result = _service.GetUserById(1);

            Assert.NotNull(result);
            Assert.Equal(1, result!.UserId);
        }

        [Fact]
        public void IsLoginValid_CorrectCredentials_ReturnTrue()
        {
            _mockRepo.Setup(r => r.IsloginValid("a", "123"))
                     .Returns(true);

            var result = _service.IsLoginValid("a", "123");

            Assert.True(result);
        }

        [Fact]
        public void IsLoginValid_WrongCredentials_ReturnFalse()
        {
            _mockRepo.Setup(r => r.IsloginValid("a", "wrong"))
                     .Returns(false);

            var result = _service.IsLoginValid("a", "wrong");

            Assert.False(result);
        }

        [Fact]
        public void UpdateProfile_CallRepoOnce()
        {
            var user = new User { UserId = 1 };

            _service.UpdateProfile(user);

            _mockRepo.Verify(r => r.UpdateProfile(user), Times.Once);
        }

        [Fact]
        public void UpdateUser_CallRepoOnce()
        {
            var user = new User { UserId = 1 };

            _service.UpdateUser(user);

            _mockRepo.Verify(r => r.UpdateUser(user), Times.Once);
        }
    }
}
