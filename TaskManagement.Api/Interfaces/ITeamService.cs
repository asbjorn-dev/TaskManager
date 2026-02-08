using System;
using TaskManagement.Api.Dtos.Teams;

namespace TaskManagement.Api.Interfaces;

public interface ITeamService
{
    Task<IEnumerable<TeamResponseDto>> GetAllAsync();
    Task<TeamResponseDto?> GetByIdAsync(Guid id);
    Task<TeamResponseDto> CreateAsync(CreateTeamDto dto);
    Task<TeamResponseDto?> UpdateAsync (Guid id, UpdateTeamDto dto);
    Task<bool> DeleteAsync(Guid id);
}
