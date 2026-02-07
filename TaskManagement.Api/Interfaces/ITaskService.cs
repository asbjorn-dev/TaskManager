using TaskManagement.Api.Dtos.Tasks;

namespace TaskManagement.Api.Interfaces;

public interface ITaskService
{
    Task<IEnumerable<TaskResponseDto>> GetAllAsync();
    Task<TaskResponseDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<TaskResponseDto>> GetByProjectIdAsync(Guid projectId);
    Task<TaskResponseDto> CreateAsync(CreateTaskDto dto);
    Task<TaskResponseDto?> UpdateAsync(Guid id, UpdateTaskDto dto);
    Task<bool> DeleteAsync(Guid id);
}