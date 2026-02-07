namespace TaskManagement.Api.Models
{
    public class Tag
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }

        // En navigationsegenskab for relaterede opgaver
        public ICollection<TaskTag> TaskTags { get; set; } = new List<TaskTag>();
    }
}
