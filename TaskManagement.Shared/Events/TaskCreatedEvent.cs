using System;

namespace TaskManagement.Shared.Events;

public class TaskCreatedEvent
{
    public Guid TaskId {get; set;}
    public string Title {get; set;} = null!;
    public Guid ProjectId {get; set;}
    public DateTime? DueDate {get; set;}
    public Guid? AssignedUserId  {get; set;}
    public DateTime CreatedAt {get; set;}
    public string AssignedUserEmail {get; set;}

}
