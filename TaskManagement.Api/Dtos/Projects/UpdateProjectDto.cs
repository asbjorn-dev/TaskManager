using TaskManagement.Api.Models.Enums;

namespace TaskManagement.Api.Dtos.Projects
{
    public class UpdateProjectDto
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
        public ProjectStatus Status { get; set; }
    }
}
