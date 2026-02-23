using System;

namespace TaskManagement.Notifications.Handlers;

public interface IEmailService
{
    Task SendTaskCreatedAsync(string toEmail, string taskTitle, DateTime? dueDate);
    Task SendTaskDueSoonAsync(string toEmail, string taskTitle, int HoursUntilDue);
}
