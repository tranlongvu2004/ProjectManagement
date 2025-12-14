using Moq;
using PorjectManagement.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace PorjectManagement.Testing.Unit_test
{
    public class InternServiceTests
    {
        private readonly Mock<IInternRepo> _mockRepo;
        private readonly InternService _service;

        public InternServiceTests()
        {
            _mockRepo = new Mock<IInternRepo>();
            _service = new InternService(_mockRepo.Object);
        }

        [Fact]
        public async System.Threading.Tasks.Task GetInternsAsync_ReturnsInternList()
        {
            // Arrange
            var interns = new List<User>
        {
            new User { UserId = 1, FullName = "Intern A", RoleId = 2 }
        };

            _mockRepo
                .Setup(r => r.GetInternsAsync())
                .ReturnsAsync(interns);

            // Act
            var result = await _service.GetInternsAsync();

            // Assert
            Assert.Single(result);
            Assert.Equal("Intern A", result.First().FullName);
        }

        [Fact]
        public async System.Threading.Tasks.Task GetInternsAsync_CallsRepositoryOnce()
        {
            // Arrange
            _mockRepo
                .Setup(r => r.GetInternsAsync())
                .ReturnsAsync(new List<User>());

            // Act
            await _service.GetInternsAsync();

            // Assert
            _mockRepo.Verify(r => r.GetInternsAsync(), Times.Once);
        }
    }

}
