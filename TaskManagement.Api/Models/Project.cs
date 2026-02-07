using TaskManagement.Api.Models.Enums;

namespace TaskManagement.Api.Models
{
    // Alle models er databasemodeller, der repræsenterer tabeller i databasen senere
    public class Project
    {
        public Guid Id { get; set; }
        public Guid TeamId { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public ProjectStatus Status { get; set; } = ProjectStatus.Active; // active or Archived
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // En navigationsegenskab for relaterede opgaver
        public Team Team { get; set; } = null!;
        public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
    }
}
