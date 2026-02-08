using System;
using TaskManagement.Api.Models;

namespace TaskManagement.Api.Interfaces;

public interface ITeamRepository
{
    Task<IEnumerable<Team>> GetAllAsync();
    Task<Team?> GetByIdAsync(Guid id);
    Task<Team> AddAsync(Team team);
    Task<Team?> UpdateAsync(Team team);
    Task<bool> DeleteAsync(Guid id);
}
