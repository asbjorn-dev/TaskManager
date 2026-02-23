using System;

namespace TaskManagement.Shared.Events;

public class TaskDueSoonEvent
{
    public Guid TaskId { get; set; }
    public string Title { get; set; } = null!;
    public DateTime DueDate { get; set; }
    public Guid? AssignedUserId { get; set; }
    public int HoursUntilDue { get; set; }
    public string AssignedUserEmail {get; set;}
}
