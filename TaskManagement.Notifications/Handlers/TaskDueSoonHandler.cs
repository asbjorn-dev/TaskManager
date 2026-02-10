using System;
using System.Text.Json;
using TaskManagement.Shared.Events;

namespace TaskManagement.Notifications.Handlers;

public class TaskDueSoonHandler : IEventHandler
{
    private readonly ILogger<TaskDueSoonHandler> _logger;
    public TaskDueSoonHandler(ILogger<TaskDueSoonHandler> logger)
    {
        _logger = logger;
    }


    public string RoutingKey => "task.due-soon";

    public Task HandleAsync(string json)
    {
        // deserialiser json til TaskDueToonEvent
        var dueSoon = JsonSerializer.Deserialize<TaskDueSoonEvent>(json);

        _logger.LogInformation(
            "[Notification] Task due soon: {Title} in {Hours} hours!",
            dueSoon?.Title, dueSoon?.HoursUntilDue
            );

        return Task.CompletedTask;
    }
}
