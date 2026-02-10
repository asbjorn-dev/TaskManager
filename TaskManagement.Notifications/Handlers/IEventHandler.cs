using System;

namespace TaskManagement.Notifications.Handlers;

// fælles interface for alle event handlers
public interface IEventHandler
{
    string RoutingKey { get; } // handlers skal sæt routingKey (f.eks. "task.created", "task.due.soon")
    Task HandleAsync(string json);
}
