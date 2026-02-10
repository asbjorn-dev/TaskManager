using TaskManagement.Api.Data;
using TaskManagement.Api.Dtos.Tasks;
using TaskManagement.Api.Interfaces;
using TaskManagement.Api.Helpers;
using TaskManagement.Api.Models;
using TaskManagement.Shared.Events;


namespace TaskManagement.Api.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _repository;
    private readonly TaskServiceHelper _taskServiceHelper;
    private readonly IEventPublisher _eventPublisher;
    
    public TaskService(ITaskRepository repository, TaskServiceHelper taskServiceHelper, IEventPublisher eventPublisher)
    {
        _repository = repository;
        _taskServiceHelper = taskServiceHelper;
        _eventPublisher = eventPublisher;
    }
    
    
    public async Task<IEnumerable<TaskResponseDto>> GetAllAsync()
    {
        var tasks = await _repository.GetAllAsync();

        return tasks.Select(_taskServiceHelper.MapToDto);
    }

    public async Task<TaskResponseDto?> GetByIdAsync(Guid id)
    {
        var taskById = await _repository.GetByIdAsync(id);
        
        return taskById == null ? null : _taskServiceHelper.MapToDto(taskById);
    }

    public async Task<IEnumerable<TaskResponseDto>> GetByProjectIdAsync(Guid projectId)
    {
        var taskByProjectId = await _repository.GetByProjectIdAsync(projectId);
        
        return taskByProjectId.Select(_taskServiceHelper.MapToDto);
    }

    public async Task<TaskResponseDto> CreateAsync(CreateTaskDto dto)
    {
        // validere at projekt findes
        var projectExists = await _repository.ProjectExistsAsync(dto.ProjectId);
        if (!projectExists)
        {
            throw new ArgumentException($"Project with ID {dto.ProjectId} does not exist.");
        }
        
        // validere at user findes (hvis user er assigned)
        if (dto.AssignedUserId.HasValue)
        {
            var userExists = await _repository.UserExistsAsync(dto.AssignedUserId.Value);
            if (!userExists)
            {
                throw new ArgumentException($"User with ID {dto.AssignedUserId} does not exist.");
            }
        }
        
        // map DTO til model
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            ProjectId = dto.ProjectId,
            Title = dto.Title,
            Description = dto.Description,
            Priority = dto.Priority,
            DueDate = dto.DueDate,
            AssignedUserId = dto.AssignedUserId,
            CreatedAt = DateTime.UtcNow,
        };

        var createdTask = await _repository.AddAsync(task);
        
        // håndtere tags (hvis de er med)
        if (dto.Tags != null && dto.Tags.Any())
        {
            await _taskServiceHelper.AddTagsToTaskAsync(createdTask.Id, dto.Tags);
        }

        // reload task med tags for at returnere korrekt dto og sikrer et response der med alle tags
        // nødvendig fordi tags tilføjes efter task creation
        var taskWithTags = await _repository.GetByIdAsync(createdTask.Id);

        // Publish event til RabbitMQ
        await _eventPublisher.PublishAsync(new TaskCreatedEvent
        {
            TaskId = task.Id,
            Title = task.Title,
            ProjectId = task.ProjectId,
            DueDate = task.DueDate,
            AssignedUserId = task.AssignedUserId,
            CreatedAt = task.CreatedAt
        }, "task.created");
        
        return _taskServiceHelper.MapToDto(taskWithTags!);
    }

    public async Task<TaskResponseDto?> UpdateAsync(Guid id, UpdateTaskDto dto)
    {
        var task = await _repository.GetByIdAsync(id);
        if (task == null)
            throw new ArgumentException($"Task with ID {id} does not exist."); 
        
        // validere at user findes (hvis den er assigned)
        if (dto.AssignedUserId.HasValue)
        {
            var userExists = await _repository.UserExistsAsync(dto.AssignedUserId.Value);
            if (!userExists)
                throw new ArgumentException($"User with ID {dto.AssignedUserId} does not exist.");
        }
        
        // update properties fra client til model
        task.Title = dto.Title;
        task.Description = dto.Description;
        task.State = dto.State;
        task.Priority = dto.Priority;
        task.DueDate = dto.DueDate;
        task.AssignedUserId = dto.AssignedUserId;
        task.UpdatedAt = DateTime.UtcNow;
        
        var updatedTask = await _repository.UpdateAsync(task);
        
        // håndtere at update tags hvis de er sendt med
        if (dto.Tags != null)
            await _taskServiceHelper.UpdateTaskTagsAsync(updatedTask.Id, dto.Tags);
        
        // reload task med opdaterede tags 
        var taskWithTags = await _repository.GetByIdAsync(updatedTask.Id);
        return _taskServiceHelper.MapToDto(taskWithTags!);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        return await _repository.DeleteAsync(id);
    }
}