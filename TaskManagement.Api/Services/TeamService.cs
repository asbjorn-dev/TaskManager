using System;
using TaskManagement.Api.Controllers;
using TaskManagement.Api.Dtos.Teams;
using TaskManagement.Api.Interfaces;
using TaskManagement.Api.Models;

namespace TaskManagement.Api.Services;

public class TeamService : ITeamService
{
    private readonly ITeamRepository _repository;
    private readonly ICacheService _cacheService;
    public TeamService(ITeamRepository repository, ICacheService cacheService)
    {
        _repository = repository;
        _cacheService = cacheService;
    }

    public async Task<IEnumerable<TeamResponseDto>> GetAllAsync()
    {
        // tjek redis om teams:all findes
        var cachedTeams = await _cacheService.GetAsync<IEnumerable<TeamResponseDto>>("teams:all");
        if (cachedTeams != null)
            return cachedTeams;

        // cache miss - udgrav fra db
        var teams = await _repository.GetAllAsync();
        var result = teams.Select(MapToDto).ToList();

        //cache teams til redis
        await _cacheService.SetAsync("teams:all", result);

        return result;
    }

   public async Task<TeamResponseDto?> GetByIdAsync(Guid id)
    {
        // tjek redis om teams:{id} findes allerede
        var cacheKey = $"teams:{id}";
        var cachedTeam = await _cacheService.GetAsync<TeamResponseDto>(cacheKey);
        if (cachedTeam != null)
            return cachedTeam;

        // cache miss - hent fra db
        var team = await _repository.GetByIdAsync(id);
        if (team == null)
            return null;
        
        var result = MapToDto(team);

        // cache enkelte team til redis
        await _cacheService.SetAsync(cacheKey, result);

        return result;
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

        // invalidate cache - Opret team skal vi clear teams:all cache
        await _cacheService.RemoveAsync("teams:all");

        return MapToDto(teamCreated);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var TeamToDelete = await _repository.DeleteAsync(id);

        // hvis succesfuld delete fra db, clear cache for teams:all og team vi har slettet "teams:{id}" 
        if (TeamToDelete)
        {
            await _cacheService.RemoveAsync("teams:all");
            await _cacheService.RemoveAsync($"teams:{id}");
        }

        return TeamToDelete;
    }

    public async Task<TeamResponseDto?> UpdateAsync(Guid id, UpdateTeamDto dto)
    {
        var team = await _repository.GetByIdAsync(id);
        if (team is null)
            return null;
            
        team.Name = dto.Name;
        
        var updated = await _repository.UpdateAsync(team);

        // invalidate cache - når update teams, clear teams:all + teams:{id} cache
        await _cacheService.RemoveAsync("teams:all");
        await _cacheService.RemoveAsync($"teams:{id}");

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
