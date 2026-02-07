using TaskManagement.Api.Dtos.Projects;
using TaskManagement.Api.Models;

namespace TaskManagement.Api.Interfaces
{
    public interface IProjectService
    {
        Task<IEnumerable<ProjectResponseDto>> GetAllAsync();
        Task<ProjectResponseDto?> GetByIdAsync(Guid id);
        Task<ProjectResponseDto> CreateAsync(CreateProjectDto dto);
        Task<ProjectResponseDto?> UpdateAsync(Guid id, UpdateProjectDto dto);
        Task<bool> DeleteAsync(Guid id);
    }
}
