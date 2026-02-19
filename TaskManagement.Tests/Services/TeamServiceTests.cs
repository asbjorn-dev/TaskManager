using Moq;
using TaskManagement.Api.Dtos.Teams;
using TaskManagement.Api.Interfaces;
using TaskManagement.Api.Models;
using TaskManagement.Api.Services;

namespace TaskManagement.Tests.Services;

public class TeamServiceTests
{
    private readonly Mock<ITeamRepository> _mockRepository;
    private readonly TeamService _teamService;

public TeamServiceTests()
{
    _mockRepository = new Mock<ITeamRepository>();
    var mockCacheService = new Mock<ICacheService>();
    // Moq returnerer empty IEnumerable som default for collection types - tving cache miss med null
    mockCacheService.Setup(c => c.GetAsync<IEnumerable<TeamResponseDto>>(It.IsAny<string>()))
        .ReturnsAsync((IEnumerable<TeamResponseDto>?)null);
    _teamService = new TeamService(_mockRepository.Object, mockCacheService.Object);
}



     // == GetAllAsync Tests ==
    [Fact]
    public async Task GetAllAsync_ReturnsAllTeams()
    {
        // Arrange
        var teams = new List<Team>
        {
            new Team { Id = Guid.NewGuid(), Name = "Platform Team", CreatedAt = DateTime.UtcNow, Projects = new List<Project>() },
            new Team { Id = Guid.NewGuid(), Name = "Frontend Team", CreatedAt = DateTime.UtcNow, Projects = new List<Project>() }
        };

        _mockRepository
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(teams);

        // Act
        var result = await _teamService.GetAllAsync();

        // Assert
        var dtos = result.ToList();
        Assert.Equal(2, dtos.Count);
        Assert.Equal("Platform Team", dtos[0].Name);
        Assert.Equal("Frontend Team", dtos[1].Name);
    }

    // == CreateAsync Tests ==
    [Fact]
    public async Task CreateAsync_ReturnsCreatedTeam()
    {
        // Arrange
        var dto = new CreateTeamDto { Name = "New Team" };

        _mockRepository
            .Setup(r => r.AddAsync(It.IsAny<Team>()))
            .ReturnsAsync((Team t) => t);

        // Act
        var result = await _teamService.CreateAsync(dto);

        // Assert
        Assert.Equal("New Team", result.Name);
        Assert.NotEqual(Guid.Empty, result.Id);
    }


    // == DeleteAsync Tests ==
    [Fact]
    public async Task DeleteAsync_ReturnsTrue_WhenDeleted()
    {
        // Arrange
        _mockRepository
            .Setup(r => r.DeleteAsync(It.IsAny<Guid>()))
            .ReturnsAsync(true);

        // Act
        var result = await _teamService.DeleteAsync(Guid.NewGuid());

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenNotFound()
    {
        // Arrange
        _mockRepository
            .Setup(r => r.DeleteAsync(It.IsAny<Guid>()))
            .ReturnsAsync(false);

        // Act
        var result = await _teamService.DeleteAsync(Guid.NewGuid());

        // Assert
        Assert.False(result);
    }
}
