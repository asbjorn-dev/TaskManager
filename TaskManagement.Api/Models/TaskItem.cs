using TaskManagement.Api.Models.Enums;

namespace TaskManagement.Api.Models
{
    public class TaskItem
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        public required string Title { get; set; }
        public string? Description { get; set; }
        public TaskState State { get; set; } = TaskState.Todo; // can be Todo, InProgress, Done or Blocked
        public TaskPriority Priority { get; set; } = TaskPriority.Medium; // can be Low, Medium or High
        public DateTime? DueDate { get; set; }
        public Guid? AssignedUserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // En navigationsegenskab for relaterede entiteter
        public Project Project { get; set; } = null!;
        public User? AssignedUser { get; set; }
        public ICollection<TaskTag> TaskTags { get; set; } = new List<TaskTag>();
    }
}
