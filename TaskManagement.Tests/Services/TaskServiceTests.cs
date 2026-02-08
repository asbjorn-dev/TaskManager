using Microsoft.EntityFrameworkCore;
using Moq;
using TaskManagement.Api.Data;
using TaskManagement.Api.Dtos.Tasks;
using TaskManagement.Api.Helpers;
using TaskManagement.Api.Interfaces;
using TaskManagement.Api.Models;
using TaskManagement.Api.Models.Enums;
using TaskManagement.Api.Services;


namespace TaskManagement.Tests.Services;

public class TaskServiceTests
{
    private readonly Mock<ITaskRepository> _mockRepository;
    private readonly TaskService _taskService;

    public TaskServiceTests()
    {
        _mockRepository = new Mock<ITaskRepository>();

        // TaskServiceHelper er en konkret klasse (har ikke interface) og den afhænger af AppDbContext
        // Derfor bruger vi InMemory database i stedet for mock
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var context = new AppDbContext(options);
        var helper = new TaskServiceHelper(context);

        _taskService = new TaskService(_mockRepository.Object, helper);
    }

    // == GetAllAsync Tests ==
    [Fact]
    public async Task GetAllAsync_ReturnsAllTasks()
    {
        // Arrange
        var tasks = new List<TaskItem>
        {
            new TaskItem
            {
                Id = Guid.NewGuid(),
                ProjectId = Guid.NewGuid(),
                Title = "Task 1",
                State = TaskState.Todo,
                Priority = TaskPriority.High,
                CreatedAt = DateTime.UtcNow,
                // uden dette vil MapToDTO kaste NullReferenceException fordi den kalder .Select()
                // på null. I API sætter EF core automatisk via Include()
                TaskTags = new List<TaskTag>() 
            },
            new TaskItem
            {
                Id = Guid.NewGuid(),
                ProjectId = Guid.NewGuid(),
                Title = "Task 2",
                State = TaskState.Done,
                Priority = TaskPriority.Low,
                CreatedAt = DateTime.UtcNow,
                TaskTags = new List<TaskTag>()
            }
        };

        // Fortæl mock: "Når GetAllAsync() kaldes, returner tasks listen med TaskTags inkluderet"
        _mockRepository
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(tasks);

        // Act
        var result = await _taskService.GetAllAsync();

        // Assert
        var dtos = result.ToList();
        Assert.Equal(2, dtos.Count);
        Assert.Equal("Task 1", dtos[0].Title);
        Assert.Equal(TaskState.Done, dtos[1].State);
    }


    // == CreateAsync Tests ==
    [Fact]
    public async Task CreateAsync_ThrowsException_WhenProjectNotFound()
    {
        // Arrange
        var dto = new CreateTaskDto
        {
            ProjectId = Guid.NewGuid(),
            Title = "New Task"
        };

        _mockRepository
            .Setup(r => r.ProjectExistsAsync(dto.ProjectId))
            .ReturnsAsync(false);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _taskService.CreateAsync(dto)
        );
    }

    [Fact]
    public async Task CreateAsync_ThrowsException_WhenUserNotFound()
    {
        // Arrange
        var dto = new CreateTaskDto
        {
            ProjectId = Guid.NewGuid(),
            Title = "New Task",
            AssignedUserId = Guid.NewGuid()
        };

        _mockRepository
            .Setup(r => r.ProjectExistsAsync(dto.ProjectId))
            .ReturnsAsync(true); // project findes

        _mockRepository
            .Setup(r => r.UserExistsAsync(dto.AssignedUserId.Value))
            .ReturnsAsync(false); // men user findes IKKE

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _taskService.CreateAsync(dto)
        );
    }


     // == DeleteAsync Tests ==
    [Fact]
    public async Task DeleteAsync_ReturnsTrue_WhenDeleted()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        _mockRepository
            .Setup(r => r.DeleteAsync(taskId))
            .ReturnsAsync(true);

        // Act
        var result = await _taskService.DeleteAsync(taskId);

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
        var result = await _taskService.DeleteAsync(Guid.NewGuid());

        // Assert
        Assert.False(result);
    }
}
