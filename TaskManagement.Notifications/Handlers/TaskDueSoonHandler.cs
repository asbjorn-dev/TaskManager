using System;
using System.Text.Json;
using TaskManagement.Shared.Events;

namespace TaskManagement.Notifications.Handlers;

public class TaskDueSoonHandler : IEventHandler
{
    private readonly IEmailService _emailService;
    private readonly ILogger<TaskDueSoonHandler> _logger;
    public TaskDueSoonHandler(IEmailService emailService, ILogger<TaskDueSoonHandler> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }


    public string RoutingKey => "task.due-soon";

    public async Task HandleAsync(string json)
    {
        // deserialiser json til TaskDueToonEvent
        var dueSoon = JsonSerializer.Deserialize<TaskDueSoonEvent>(json);
        if (dueSoon == null)
            return;

        _logger.LogInformation(
            "[Notification] Task due soon: {Title} in {Hours} hours!",
            dueSoon?.Title, dueSoon?.HoursUntilDue
            );

        if (!string.IsNullOrEmpty(dueSoon.AssignedUserEmail))
            await _emailService.SendTaskDueSoonAsync(dueSoon.AssignedUserEmail, dueSoon.Title, dueSoon.HoursUntilDue);
    }
}
