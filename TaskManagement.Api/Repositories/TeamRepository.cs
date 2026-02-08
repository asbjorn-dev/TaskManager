using System;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Api.Data;
using TaskManagement.Api.Interfaces;
using TaskManagement.Api.Models;

namespace TaskManagement.Api.Repositories;

public class TeamRepository : ITeamRepository
{
    private readonly AppDbContext _context;
    public TeamRepository(AppDbContext context)
    {
        _context = context;
    }


    public async Task<IEnumerable<Team>> GetAllAsync()
    {
        return await _context.Teams
        .Include(t => t.Projects)
        .ToListAsync();
    }

    public async Task<Team?> GetByIdAsync(Guid id)
    {
         return await _context.Teams
        .Include(t => t.Projects)
        .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<Team> AddAsync(Team team)
    {
        _context.Teams.Add(team);
        await _context.SaveChangesAsync();
        return team;
    }

    public async Task<Team?> UpdateAsync(Team team)
    {
        _context.Teams.Update(team);
        await _context.SaveChangesAsync();
        return team;
    }

    // cascade delete er konfigureret i migration - slet 1 team, sletter alle dens projects og taskitems.
    public async Task<bool> DeleteAsync(Guid id)
    {
        var teamToDelete = await _context.Teams.FindAsync(id);
        if (teamToDelete == null)
            return false;

        _context.Teams.Remove(teamToDelete);
        await _context.SaveChangesAsync();
        return true;
    }

}
