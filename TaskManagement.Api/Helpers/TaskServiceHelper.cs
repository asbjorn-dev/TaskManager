using Microsoft.EntityFrameworkCore;
using TaskManagement.Api.Data;
using TaskManagement.Api.Dtos.Tasks;
using TaskManagement.Api.Models;

namespace TaskManagement.Api.Helpers;

public class TaskServiceHelper
{
    private readonly AppDbContext _context;
    
    public TaskServiceHelper(AppDbContext context)
    {
        _context = context; // bruges kun til tag (så client ik skal sende Guid, men en liste<string>
    }
    
    
    public TaskResponseDto MapToDto(TaskItem task)
    {
        return new TaskResponseDto
        {
            Id = task.Id,
            ProjectId = task.ProjectId,
            Title = task.Title,
            Description = task.Description,
            State = task.State,
            Priority = task.Priority,
            DueDate = task.DueDate,
            AssignedUserId = task.AssignedUserId,
            CreatedAt = task.CreatedAt,
            UpdatedAt = task.UpdatedAt,
            Tags = task.TaskTags.Select(tt => tt.Tag.Name).ToList()  // ← Many-to-many mapping
        };
    }
    
    public async Task AddTagsToTaskAsync(Guid taskId, List<string> tagNames)
    {
        foreach (var tagName in tagNames)
        {
            // Find eller opret tag - gør det muligt for at klient ikke skal kende tag Ids)
            var tag = await _context.Tags.FirstOrDefaultAsync(t => t.Name == tagName);
            if (tag == null)
            {
                tag = new Tag { Id = Guid.NewGuid(), Name = tagName };
                _context.Tags.Add(tag);
                await _context.SaveChangesAsync();
            }

            // Opret TaskTag join
            var taskTag = new TaskTag
            {
                TaskItemId = taskId,
                TagId = tag.Id
            };
            _context.TaskTags.Add(taskTag);
        }

        await _context.SaveChangesAsync();
    }
    
    // replace strategy
    public async Task UpdateTaskTagsAsync(Guid taskId, List<string> tagNames)
    {
        // Fjern alle eksisterende tags for denne task
        var existingTaskTags = _context.TaskTags.Where(tt => tt.TaskItemId == taskId);
        _context.TaskTags.RemoveRange(existingTaskTags);
        await _context.SaveChangesAsync();

        // Tilføj de nye tags
        if (tagNames.Any())
        {
            await AddTagsToTaskAsync(taskId, tagNames);
        }
    }
}