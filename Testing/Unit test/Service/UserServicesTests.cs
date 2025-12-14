using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PorjectManagement.Models;
using PorjectManagement.Repository.Interface;
using PorjectManagement.Service;
using System;

namespace Testing.UnitTest
{
    [TestClass]
    public class UserServicesTests
    {
        private Mock<IUserRepo> _mockRepo = null!;
        private UserServices _service = null!;

        [TestInitialize]
        public void Setup()
        {
            _mockRepo = new Mock<IUserRepo>();
            _service = new UserServices(_mockRepo.Object);
        }
        [TestMethod]
        public void CreateAccount_ValidUser_ReturnUser()
        {
            var user = new User { Email = "a@gmail.com" };

            _mockRepo.Setup(r => r.CreateAccount(user))
                     .Returns(user);

            var result = _service.CreateAccount(user);

            Assert.IsNotNull(result);
            Assert.AreEqual("a@gmail.com", result.Email);
            _mockRepo.Verify(r => r.CreateAccount(user), Times.Once);
        }
        [TestMethod]
        public void GetUser_EmailExists_ReturnUser()
        {
            _mockRepo.Setup(r => r.GetUserByEmail("test@gmail.com"))
                     .Returns(new User { Email = "test@gmail.com" });

            var result = _service.GetUser("test@gmail.com");

            Assert.IsNotNull(result);
            Assert.AreEqual("test@gmail.com", result.Email);
        }
        [TestMethod]
        public void GetUserById_ValidId_ReturnUser()
        {
            _mockRepo.Setup(r => r.getUserById(1))
                     .Returns(new User { UserId = 1 });

            var result = _service.GetUserById(1);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.UserId);
        }
        [TestMethod]
        public void IsLoginValid_CorrectCredentials_ReturnTrue()
        {
            _mockRepo.Setup(r => r.IsloginValid("a", "123"))
                     .Returns(true);

            var result = _service.IsLoginValid("a", "123");

            Assert.IsTrue(result);
        }
        [TestMethod]
        public void IsLoginValid_WrongCredentials_ReturnFalse()
        {
            _mockRepo.Setup(r => r.IsloginValid("a", "wrong"))
                     .Returns(false);

            var result = _service.IsLoginValid("a", "wrong");

            Assert.IsFalse(result);
        }
        [TestMethod]
        public void UpdateProfile_CallRepoOnce()
        {
            var user = new User { UserId = 1 };

            _service.UpdateProfile(user);

            _mockRepo.Verify(r => r.UpdateProfile(user), Times.Once);
        }
        [TestMethod]
        public void UpdateUser_CallRepoOnce()
        {
            var user = new User { UserId = 1 };

            _service.UpdateUser(user);

            _mockRepo.Verify(r => r.UpdateUser(user), Times.Once);
        }
    }
}