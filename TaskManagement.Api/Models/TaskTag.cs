namespace TaskManagement.Api.Models
{
    public class TaskTag
    {
        // many-to-many relationship
        public Guid TaskItemId { get; set; }
        public TaskItem TaskItem { get; set; } = null!;
        public Guid TagId { get; set; }
        public Tag Tag { get; set; } = null!;
    }
}
