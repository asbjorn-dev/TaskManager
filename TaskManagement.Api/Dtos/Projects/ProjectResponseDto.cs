using TaskManagement.Api.Models.Enums;

namespace TaskManagement.Api.Dtos.Projects
{
    public class ProjectResponseDto
    {
        public Guid Id { get; set; }
        public Guid TeamId { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public ProjectStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
