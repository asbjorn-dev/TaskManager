namespace TaskManagement.Api.Models
{
    public class Team
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // En navigationsegenskab for relaterede projekter
        public ICollection<Project> Projects { get; set; } = new List<Project>();
    }
}
