using System;
using TaskManagement.Api.Dtos.Teams;
using TaskManagement.Api.Interfaces;
using TaskManagement.Api.Models;

namespace TaskManagement.Api.Services;

public class TeamService : ITeamService
{
    private readonly ITeamRepository _repository;
    public TeamService(ITeamRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<TeamResponseDto>> GetAllAsync()
    {
        var teams = await _repository.GetAllAsync();

        return teams.Select(MapToDto);
    }

   public async Task<TeamResponseDto?> GetByIdAsync(Guid id)
    {
        var team = await _repository.GetByIdAsync(id);
        
        return team != null ? MapToDto(team) : null;
    }

    public async Task<TeamResponseDto> CreateAsync(CreateTeamDto dto)
    {
        var teamToCreate = new Team
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            CreatedAt = DateTime.UtcNow
        };

        var teamCreated = await _repository.AddAsync(teamToCreate);
        return MapToDto(teamCreated);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        return await _repository.DeleteAsync(id);
    }

    public async Task<TeamResponseDto?> UpdateAsync(Guid id, UpdateTeamDto dto)
    {
        var team = await _repository.GetByIdAsync(id);
        if (team is null)
            return null;
            
        team.Name = dto.Name;
        
        var updated = await _repository.UpdateAsync(team);
        return MapToDto(updated);
    }

    // Team model er så simpel så giver ikke mening at lave en helper class
    private TeamResponseDto MapToDto(Team team)
    {
        return new TeamResponseDto
        {
            Id = team.Id,
            Name = team.Name,
            CreatedAt = team.CreatedAt
        };
    }
}
