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
    private readonly ICacheService _cacheService;
    
    public TaskService(ITaskRepository repository, TaskServiceHelper taskServiceHelper, IEventPublisher eventPublisher, ICacheService cacheService)
    {
        _repository = repository;
        _taskServiceHelper = taskServiceHelper;
        _eventPublisher = eventPublisher;
        _cacheService = cacheService;
    }
    
    
    public async Task<IEnumerable<TaskResponseDto>> GetAllAsync()
    {
        // tjek redis cache først (cache hit)
        var cachedTasks = await _cacheService.GetAsync<IEnumerable<TaskResponseDto>>("tasks:all");
        if (cachedTasks != null)
            return cachedTasks;

        // cache miss - hent fra db
        var tasks = await _repository.GetAllAsync();
        var result = tasks.Select(_taskServiceHelper.MapToDto).ToList();

        // gem i redis
        await _cacheService.SetAsync("tasks:all", result);

        return result;
    }

    public async Task<TaskResponseDto?> GetByIdAsync(Guid id)
    {
        // tjek redis cache om task findes 
        var cacheKey = $"tasks:{id}"; // unik cache key f.eks. "tasks:123e456..."
        var cachedTask = await _cacheService.GetAsync<TaskResponseDto>(cacheKey);
        if (cachedTask != null)
            return cachedTask;

        // cache miss - hent fra db
        var taskById = await _repository.GetByIdAsync(id);
        if (taskById == null)
            return null;
        
        // map til dto og gem i cache
        var result = _taskServiceHelper.MapToDto(taskById);
        await _cacheService.SetAsync(cacheKey, result);
        
        return result;
    }

    public async Task<IEnumerable<TaskResponseDto>> GetByProjectIdAsync(Guid projectId)
    {
        // tjek redis cache om task findes udfra specifikt projekt
        var cachekey = $"tasks:project:{projectId}";
        var cached = await _cacheService.GetAsync<IEnumerable<TaskResponseDto>>(cachekey);
        if (cached != null)
            return cached;

        // cache miss - hent fra db
        var tasks = await _repository.GetByProjectIdAsync(projectId);
        var result = tasks.Select(_taskServiceHelper.MapToDto).ToList();

        // gem i redis
        await _cacheService.SetAsync(cachekey, result);
        
        return result;
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

        // Publish event til RabbitMQ (notification service)
        await _eventPublisher.PublishAsync(new TaskCreatedEvent
        {
            TaskId = task.Id,
            Title = task.Title,
            ProjectId = task.ProjectId,
            DueDate = task.DueDate,
            AssignedUserId = task.AssignedUserId,
            CreatedAt = task.CreatedAt,
            AssignedUserEmail = taskWithTags?.AssignedUser?.Email
        }, "task.created");
        
        // invalidate cache - ny task oprettes, skal cache ryddes (forældet)
        await _cacheService.RemoveAsync("tasks:all");
        await _cacheService.RemoveAsync($"tasks:project:{task.ProjectId}");

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

        // invalidate cache - når man opdater task, fjernes nuværende cached tasks
        await _cacheService.RemoveAsync("tasks:all"); // slet alle tasks fra cache
        await _cacheService.RemoveAsync($"tasks:{id}"); // slet specifik task fra cache
        await _cacheService.RemoveAsync($"tasks:project:{task.ProjectId}"); // slet task for specifik projekt 

        return _taskServiceHelper.MapToDto(taskWithTags!);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        // GET task'en før vi slet fra db så vi ved hvad projekt cache skal slettes ved create og update
        var task = await _repository.GetByIdAsync(id);

        // slet fra db
        var deleted = await _repository.DeleteAsync(id);

        // hvis delete lykkes og task findes, slet fra cache
        if (deleted && task != null)
        {
            await _cacheService.RemoveAsync("tasks:all");
            await _cacheService.RemoveAsync($"tasks:{id}");
            await _cacheService.RemoveAsync($"tasks:project:{task.ProjectId}");
        }

        return deleted;
    }
}