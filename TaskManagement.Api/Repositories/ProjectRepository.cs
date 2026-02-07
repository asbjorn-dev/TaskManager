using Microsoft.EntityFrameworkCore;
using TaskManagement.Api.Data;
using TaskManagement.Api.Interfaces;
using TaskManagement.Api.Models;

namespace TaskManagement.Api.Repositories;

public class ProjectRepository : IProjectRepository
{
    private readonly AppDbContext _context;
    
    public ProjectRepository(AppDbContext context)
    {
        _context = context;
    }
    
    // Hent alle projekter (med Team og Tasks)
    public async Task<IEnumerable<Project>> GetAllAsync()
    {
        return await _context.Projects
            .Include(p => p.Team)
            .Include(p => p.Tasks)
            .ToListAsync();
    }
    
    // Hent et project (brugt i bl.a. update)
    public async Task<Project?> GetByIdAsync(Guid id)
    {
        return await _context.Projects
            .Include(p => p.Team)
            .Include(p => p.Tasks)
            .FirstOrDefaultAsync(p => p.Id == id);
    }
    
    // hent projekter for et team
    public async Task<IEnumerable<Project>> GetByTeamIdAsync(Guid id)
    {
        return await _context.Projects
            .Include(p => p.Team)
            .Where(p => p.TeamId == id)
            .ToListAsync();
    }
    
    // tilføj nyt project
    public async Task<Project> AddAsync(Project project)
    {
        _context.Projects.Add(project);
        await _context.SaveChangesAsync();
        return project;
    }

    // opdater et project
    public async Task<Project> UpdateAsync(Project project)
    {
        _context.Projects.Update(project);
        await _context.SaveChangesAsync();
        return project;
    }
    
    // slet et project
    public async Task<bool> DeleteAsync(Guid id)
    {
        var projectToDelete = await _context.Projects.FindAsync(id);
        if (projectToDelete == null) 
            return false;
        
        _context.Projects.Remove(projectToDelete);
        await _context.SaveChangesAsync();
        return true;
    }

    // helper til validation (bruges i service til bl.a. create project)
    public async Task<bool> TeamExistsAsync(Guid teamId)
    {
        return await _context.Teams.AnyAsync(t => t.Id == teamId);
    }
}