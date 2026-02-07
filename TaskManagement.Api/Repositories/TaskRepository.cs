using Microsoft.EntityFrameworkCore;
using TaskManagement.Api.Data;
using TaskManagement.Api.Interfaces;
using TaskManagement.Api.Models;

namespace TaskManagement.Api.Repositories;

public class TaskRepository : ITaskRepository
{
    private readonly AppDbContext _context;
    
    public TaskRepository(AppDbContext context)
    {
        _context = context;
    }
    
    
    public async Task<IEnumerable<TaskItem>> GetAllAsync()
    {
        // nested ThenInclude() grundet many-to-many relationship (join entity)
        return await _context.TaskItems
                // Include() henter relaterede entiteter til TaskItems
            .Include(t => t.Project)
            .Include(t => t.AssignedUser)
                
            // TaskTags har Many-to-Many relationship med Tags så vi henter først ved join (tasktags)
            .Include(t => t.TaskTags)
              .ThenInclude(tt => tt.Tag)  // og derefter henter Tag for hver tilhørende TaskTag
            .ToListAsync();
    }

    public async Task<TaskItem?> GetByIdAsync(Guid id)
    {
        return await _context.TaskItems
            .Include(t => t.Project)
            .Include(t => t.AssignedUser)
            .Include(t => t.TaskTags)
              .ThenInclude(tt => tt.Tag)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<IEnumerable<TaskItem>> GetByProjectIdAsync(Guid projectId)
    {
        return await _context.TaskItems
            .Include(t => t.AssignedUser)
            .Include(t => t.TaskTags)
              .ThenInclude(tt => tt.Tag)
            .Where(t => t.ProjectId == projectId) // db filter istedet for c# memory
            .ToListAsync();
        
    }

    public async Task<IEnumerable<TaskItem>> GetByUserIdAsync(Guid userId)
    {
        return await _context.TaskItems
            .Include(t => t.Project)
            .Include(t => t.TaskTags)
              .ThenInclude(tt => tt.Tag)
            // nullable foreign key filter til db med Where() - henter kun tasks der er tildelt en bruger
            .Where(t => t.AssignedUserId == userId) 
            .ToListAsync();
    }

    public async Task<TaskItem> AddAsync(TaskItem task)
    {
        _context.TaskItems.Add(task);
        await _context.SaveChangesAsync();
        return task;
    }

    public async Task<TaskItem> UpdateAsync(TaskItem task)
    {
        _context.TaskItems.Update(task);
        await _context.SaveChangesAsync();
        return task;
        
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var taskToDelete = await _context.TaskItems.FindAsync(id);
        if (taskToDelete == null)
            return false;

        _context.TaskItems.Remove(taskToDelete);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ProjectExistsAsync(Guid projectId)
    {
        return await _context.Projects.AnyAsync(p => p.Id == projectId);
    }

    public async Task<bool> UserExistsAsync(Guid userId)
    {
        return await _context.Users.AnyAsync(u => u.Id == userId);
    }
}