namespace TaskManagement.Api.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string Email { get; set; }

        // En navigationsegenskab for relaterede opgaver
        public ICollection<TaskItem> AssignedTasks { get; set; } = new List<TaskItem>();
    }
}
