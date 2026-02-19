using Moq;
using TaskManagement.Api.Dtos.Projects;
using TaskManagement.Api.Interfaces;
using TaskManagement.Api.Models;
using TaskManagement.Api.Models.Enums;
using TaskManagement.Api.Services;

namespace TaskManagement.Tests.Services;

// Bruger Moq for at undgå unødvendig kald til db ved at mock med fake dependencies
// Bruger AAA pattern (arrange, act, assert). Act kalder metode, Act kalder metode man tester og 
// Assert vertificerer resultat er korrekt
// Navngivning på tests: "Method_Result_Condition" 
public class ProjectServiceTests
{
    private readonly Mock<IProjectRepository> _mockRepository;
    private readonly ProjectService _projectService;
    public ProjectServiceTests()
    {
        _mockRepository = new Mock<IProjectRepository>();
        var mockCacheService = new Mock<ICacheService>();
        _projectService = new ProjectService(_mockRepository.Object, mockCacheService.Object);
    }

    // == GetAllAsync Tests == 
    // Fact til test der køre en gang med faste værdier. Theory kører samme test med forskellige input data med [InLineData]
    [Fact]
    public async Task GetAllAsync_ReturnsAllProjects()
    {
        // Arrange - opsæt test data
        var projects = new List<Project>
        {
            new Project
            {
                Id = Guid.NewGuid(),
                TeamId = Guid.NewGuid(),
                Name = "Project 1",
                Status = ProjectStatus.Active,
                CreatedAt = DateTime.UtcNow
            },
            new Project
            {
                Id = Guid.NewGuid(),
                TeamId = Guid.NewGuid(),
                Name = "Project 2",
                Status = ProjectStatus.Archived,
                CreatedAt = DateTime.UtcNow
            }
        };

        // Fortæller moq - Når GetAllAsync() kaldes, returner denne liste
        _mockRepository
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(projects);

        // Act - kør metoden vi tester
        var result = await _projectService.GetAllAsync();

        // Assert - verificer resultatet
        var dtos = result.ToList();
        Assert.Equal(2, dtos.Count);
        Assert.Equal("Project 1", dtos[0].Name);
        Assert.Equal("Project 2", dtos[1].Name);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsEmptyList_WhenNoProjects()
    {
        // Arrange
        _mockRepository
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Project>());

        // Act
        var result = await _projectService.GetAllAsync();

        // Assert
        Assert.Empty(result);
    }


    // == GetByIdAsync  Tests == 
     [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
    {
        // Arrange
        var projectId = Guid.NewGuid();

        _mockRepository
            .Setup(r => r.GetByIdAsync(projectId))
            .ReturnsAsync((Project?)null);  // returner null

        // Act
        var result = await _projectService.GetByIdAsync(projectId);

        // Assert
        Assert.Null(result);
    }


    // == CreateAsync Tests ==
     [Fact]
    public async Task CreateAsync_ThrowsException_WhenTeamNotFound()
    {
        // Arrange
        var dto = new CreateProjectDto
        {
            TeamId = Guid.NewGuid(),
            Name = "New Project"
        };

        _mockRepository
            .Setup(r => r.TeamExistsAsync(dto.TeamId))
            .ReturnsAsync(false);  // team findes ikke

        // Act & Assert
        // skal ramme if (!teamExist) i CreateAsync()
        await Assert.ThrowsAsync<Exception>( // forventer den kaster exception
            () => _projectService.CreateAsync(dto) 
        );
    }

    
    // == UpdateAsync Test ==
    [Fact]
    public async Task UpdateAsync_ReturnsUpdatedProject_WhenExists()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var existingProject = new Project
        {
            Id = projectId,
            TeamId = Guid.NewGuid(),
            Name = "Old Name",
            Status = ProjectStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        var dto = new UpdateProjectDto
        {
            Name = "New Name",
            Description = "New Description",
            Status = ProjectStatus.Archived
        };

        _mockRepository
            .Setup(r => r.GetByIdAsync(projectId))
            .ReturnsAsync(existingProject);
        
        // It.IsAny<Project>() matcher ethvert project argument
        _mockRepository
            .Setup(r => r.UpdateAsync(It.IsAny<Project>()))
            .ReturnsAsync((Project p) => p);

        // Act
        var result = await _projectService.UpdateAsync(projectId, dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("New Name", result.Name);
        Assert.Equal("New Description", result.Description);
        Assert.Equal(ProjectStatus.Archived, result.Status);
    }


    // == DeleteAsync Tests ==
    [Fact]
    public async Task DeleteAsync_ReturnsTrue_WhenDeleted()
    {
        // Arrange
        var projectId = Guid.NewGuid();

        _mockRepository
            .Setup(r => r.DeleteAsync(projectId))
            .ReturnsAsync(true);

        // Act
        var result = await _projectService.DeleteAsync(projectId);

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
        var result = await _projectService.DeleteAsync(Guid.NewGuid());

        // Assert
        Assert.False(result);
    }
}

