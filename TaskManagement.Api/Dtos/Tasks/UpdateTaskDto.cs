using TaskManagement.Api.Models.Enums;

namespace TaskManagement.Api.Dtos.Tasks
{
    public class UpdateTaskDto
    {
        public required string Title { get; set; }
        public string? Description { get; set; }

        public TaskState State { get; set; }
        public TaskPriority Priority { get; set; }

        public DateTime? DueDate { get; set; }
        public Guid? AssignedUserId { get; set; }
        
        
        // dette gør klient sender ["backend", "urgent"] f.eks. istedet for Guids - lettere at arbejde med
        public List<string>? Tags { get; set; } // liste af tag names
    }
}
