using System;
using System.Text.Json;
using TaskManagement.Shared.Events;

namespace TaskManagement.Notifications.Handlers;

public class TaskCreatedHandler : IEventHandler
{
    private readonly ILogger<TaskCreatedHandler> _logger;
    public TaskCreatedHandler(ILogger<TaskCreatedHandler> logger)
    {
        _logger = logger;
    }

    public string RoutingKey => "task.created";

    public Task HandleAsync(string json)
    {
        // convert JSON til TaskCreatedEvent objekt
        var created = JsonSerializer.Deserialize<TaskCreatedEvent>(json);

        _logger.LogInformation(
            "[Notification] New task: {Title} (Due: {DueDate})",
            created?.Title, created?.DueDate
        );

        // return completed Task
        return Task.CompletedTask;
    }
}
