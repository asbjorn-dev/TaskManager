using System.Numerics;
using TaskManagement.Api.Models;

namespace TaskManagement.Api.Interfaces;

public interface ITaskRepository
{
    Task<IEnumerable<TaskItem>> GetAllAsync();
    
    // istedet for generisk GetAll() og filtrer i service,
    // så Get med id fordi SQL WHERE query er hurtigere end C# filtrering
    // + bruger repository pattern = services skal ikke vide hvordan data hentes
    Task<TaskItem?> GetByIdAsync(Guid id);
    Task<IEnumerable<TaskItem>> GetByProjectIdAsync(Guid projectId);
    Task<IEnumerable<TaskItem>> GetByUserIdAsync(Guid userId);
    
    Task<TaskItem> AddAsync(TaskItem task);
    Task<TaskItem> UpdateAsync(TaskItem task);
    Task<bool> DeleteAsync(Guid id);
    
    // laver checks/validation i repository og ik services fordi SSOT - repositories ejer data access logik
    Task<bool> ProjectExistsAsync(Guid projectId);
    Task<bool> UserExistsAsync(Guid userId);
}