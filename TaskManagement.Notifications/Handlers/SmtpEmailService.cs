using System;
using MailKit.Net.Smtp;
using MimeKit;

namespace TaskManagement.Notifications.Handlers;

public class SmtpEmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<SmtpEmailService> _logger;
    public SmtpEmailService(IConfiguration config, ILogger<SmtpEmailService> logger)
    {
        _config = config;
        _logger = logger;
    }


    public async Task SendTaskCreatedAsync(string toEmail, string taskTitle, DateTime? dueDate)
    {
        var subject = $"New task created: {taskTitle}";
        var body = dueDate.HasValue 
            ? $"A new task has been assigned to you: {taskTitle}\nDue: {dueDate.Value:dd/MM/yyyy} UTC"
            : $"A new task has been assigned to you: {taskTitle}";

        await SendAsync(toEmail, subject, body);
    }

    public async Task SendTaskDueSoonAsync(string toEmail, string taskTitle, int HoursUntilDue)
    {
        var subject = $"Reminder: '{taskTitle}' is due in {HoursUntilDue} hours";
        var body = $"Your task '{taskTitle}' is due in {HoursUntilDue} hours. Remember to complete it";

        await SendAsync(toEmail, subject, body);
    }

    private async Task SendAsync(string toEmail, string subject, string body)
    {
        // selve email formatet (From, To, Subject, Body)
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_config["Smtp:FromName"], _config["Smtp:FromEmail"]));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = subject;
        message.Body = new TextPart("plain") { Text = body};

        // SMTP connection lukkes altid, selv ved fejl.
        using var client = new SmtpClient();
        await client.ConnectAsync(_config["Smtp:Host"], int.Parse(_config["Smtp:Port"]!), MailKit.Security.SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(_config["Smtp:Username"], _config["Smtp:Password"]);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);

        _logger.LogInformation("[Email] Sent: '{Subject}' to {Email}", subject, toEmail);
    }
}
