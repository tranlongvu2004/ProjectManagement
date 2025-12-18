using Moq;
using Xunit;
using PorjectManagement.Models;
using PorjectManagement.Service;
using System.Collections.Generic;
using System.Threading.Tasks;

public class InternServiceTests
{
    private readonly Mock<IInternRepo> _internRepoMock;
    private readonly InternService _service;

    public InternServiceTests()
    {
        _internRepoMock = new Mock<IInternRepo>();
        _service = new InternService(_internRepoMock.Object);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetInternsAsync_ReturnsInternList()
    {
        // Arrange
        var interns = new List<User>
        {
            new User { UserId = 1, FullName = "Intern A" },
            new User { UserId = 2, FullName = "Intern B" }
        };

        _internRepoMock
            .Setup(r => r.GetInternsAsync())
            .ReturnsAsync(interns);

        // Act
        var result = await _service.GetInternsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, ((List<User>)result).Count);
        _internRepoMock.Verify(r => r.GetInternsAsync(), Times.Once);
    }
}
