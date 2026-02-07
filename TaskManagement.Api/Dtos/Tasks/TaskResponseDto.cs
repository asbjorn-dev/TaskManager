using TaskManagement.Api.Models.Enums;

namespace TaskManagement.Api.Dtos.Tasks
{
    public class TaskResponseDto
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }

        public string Title { get; set; } = null!;
        public string? Description { get; set; }

        public TaskState State { get; set; }
        public TaskPriority Priority { get; set; }

        public DateTime? DueDate { get; set; }
        public Guid? AssignedUserId { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        
        // dette gør klient sender ["backend", "urgent"] f.eks. istedet for Guids - lettere at arbejde med
        public IEnumerable<string> Tags { get; set; } = []; // liste af tag names
    }
}
