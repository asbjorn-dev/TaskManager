using System;

namespace TaskManagement.Shared.Events;

public class UserRegisteredEvent
{
    public Guid UserId {get; set;} // samme Id som Identity Service laver så Core APi kan gemme det til dens egen user db
    public string Name {get; set;} = string.Empty;
    public string Email {get; set;} = string.Empty;
}
