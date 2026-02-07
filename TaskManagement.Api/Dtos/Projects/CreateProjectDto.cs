namespace TaskManagement.Api.Dtos.Projects
{
    public class CreateProjectDto
    {
        public Guid TeamId { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
    }
}
