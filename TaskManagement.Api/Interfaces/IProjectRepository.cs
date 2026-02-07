using TaskManagement.Api.Models;

namespace TaskManagement.Api.Interfaces;

public interface IProjectRepository
{
    Task<IEnumerable<Project>> GetAllAsync();
    Task<Project?> GetByIdAsync(Guid id);
    Task<IEnumerable<Project>> GetByTeamIdAsync(Guid id);
    Task<Project> AddAsync(Project project);
    Task<Project> UpdateAsync(Project project);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> TeamExistsAsync(Guid teamId);
}