using System;
using System.Text.Json;
using TaskManagement.Shared.Events;

namespace TaskManagement.Notifications.Handlers;

public class TaskCreatedHandler : IEventHandler
{
    private readonly IEmailService _emailService;
    private readonly ILogger<TaskCreatedHandler> _logger;
    public TaskCreatedHandler(IEmailService emailService, ILogger<TaskCreatedHandler> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    public string RoutingKey => "task.created";

    public async Task HandleAsync(string json)
    {
        // convert JSON til TaskCreatedEvent objekt
        var created = JsonSerializer.Deserialize<TaskCreatedEvent>(json);
        if (created == null)
            return;

        _logger.LogInformation(
            "[Notification] New task: {Title} (Due: {DueDate})",
            created?.Title, created?.DueDate
        );

        // send kun email hvis task er assigned til en user med email
        if (!string.IsNullOrEmpty(created.AssignedUserEmail))
            await _emailService.SendTaskCreatedAsync(created.AssignedUserEmail, created.Title, created.DueDate);
    }
}
