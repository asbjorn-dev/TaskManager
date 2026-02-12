using System;
using TaskManagement.Api.Interfaces;
using TaskManagement.Shared.Events;

namespace TaskManagement.Api.Services;

// background job der tjekker for tasks med deadline inden 24 timer (scheduled job)
// Kører i baggrunden parallelt med API'en (i sin egen tråd) - blokerer ikke normale request
public class DueSoonBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IEventPublisher _publisher;
    private readonly ILogger<DueSoonBackgroundService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(30); // tjekker hvert 30 min om tasks snart udløber
    private readonly int _hoursThreshold = 24; // variable brugt til sende notification hvis deadline under 24 timer
    public DueSoonBackgroundService(IServiceScopeFactory scopeFactory, IEventPublisher publisher, ILogger<DueSoonBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _publisher = publisher;
        _logger = logger;
    }

    // hoved funktion kører når service starter
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("[DueSoon] Background service started. Checking every {Minutes} minutes",
        _checkInterval.TotalMinutes);

        // kører i en uendelig loop indtil app lukker
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckDueSoonTasksAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DueSoon] Error checking tasks");
            }

            // venter 30 min før næste iteration i loop
            await Task.Delay(_checkInterval, stoppingToken);
        }
    }

    private async Task CheckDueSoonTasksAsync()
    {
        // Opretter et scope fordi BackgroundService er Singleton
        // men vores Repository er Scoped (laver ny connection per request)
        using var scope = _scopeFactory.CreateScope();
        var taskRepository = scope.ServiceProvider.GetRequiredService<ITaskRepository>();

        // hent alle tasks fra db
        var allTasks = await taskRepository.GetAllAsync();

        var now = DateTime.UtcNow;

        // Find tasks der opfylder alle disse kriterier:
        // 1. Har en deadline (DueDate)
        // 2. Deadline er i fremtiden (ikke allerede overskredet)
        // 3. Deadline er før eller præcis de næste 24 timer
        // 4. Task er ikke allerede færdig
        var dueSoonTasks = allTasks.Where(t => t.DueDate.HasValue 
        && t.DueDate.Value > now
        && t.DueDate.Value <= now.AddHours(_hoursThreshold)
        && t.State != Models.Enums.TaskState.Done)
        .ToList();

        _logger.LogInformation("[DueSoon] Found {Count} tasks due within {Hours} hours",
        dueSoonTasks.Count, _hoursThreshold);

        // send notifikation via RabbitMQ for hver task der snart udløber
        foreach (var task in dueSoonTasks)
        {
            // trækker nuværende tid fra deadline (f.eks. 14:00 - 10:00 = 4 timer som TimeSpan)
            var hoursLeft = (int)(task.DueDate!.Value - now).TotalHours;

            // publish event til rabbit - notification lytter også på "task.due-soon" routing key
            await _publisher.PublishAsync(new TaskDueSoonEvent
            {
                TaskId = task.Id,
                Title = task.Title,
                DueDate = task.DueDate.Value,
                AssignedUserId = task.AssignedUserId,
                HoursUntilDue = hoursLeft
            }, "task.due-soon");
        }
    }
}
