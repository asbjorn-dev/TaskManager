namespace TaskManagement.Api.Dtos.Teams
{
    public class TeamResponseDto
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
