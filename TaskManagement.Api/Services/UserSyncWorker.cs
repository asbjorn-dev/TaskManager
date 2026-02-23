using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using TaskManagement.Api.Data;
using TaskManagement.Api.Infrastructure;
using TaskManagement.Api.Models;
using TaskManagement.Shared.Events;
namespace TaskManagement.Api.Services;

// lytter på user-events exchange og synch'er nye users fra identity til core api's db
public class UserSyncWorker : BackgroundService
{
    private readonly RabbitMqConnectionFactory _connectionFactory;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<UserSyncWorker> _logger;
    private const string ExchangeName = "user-events";
    private const string QueueName = "user-sync-queue";
    public UserSyncWorker(RabbitMqConnectionFactory connectionFactory, IServiceScopeFactory scopeFactory, ILogger<UserSyncWorker> logger)
    {
        _connectionFactory = connectionFactory;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var channel = _connectionFactory.CreateChannel();

        //declare exchange + queue - safe at kalde selvom de allerede eksiterer
        await channel.ExchangeDeclareAsync(ExchangeName, ExchangeType.Topic, durable: true);
        await channel.QueueDeclareAsync(QueueName, durable: true, exclusive: false, autoDelete: false);
        await channel.QueueBindAsync(QueueName, ExchangeName, "user.registered");

        // AsyncEventingBasicConsumer() er rabbitmq.Client class der lytter på queue passivt
        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (model, ea) => // add'er en funktion til ReceivedAsync event - hver gang en besked kommer, kalder rabbit denne func (ea er message med body, routingkey etc.)
        {
            // udpakker message til UserRegisteredEvent objekt
            var json = Encoding.UTF8.GetString(ea.Body.ToArray());
            var userEvent = JsonSerializer.Deserialize<UserRegisteredEvent>(json);

            if (userEvent != null)
                await SyncUserAsync(userEvent);

            // bekræft at besked er behandlet - rabbit fjerner den fra queue
            await channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
        };

        await channel.BasicConsumeAsync(QueueName, autoAck: false, consumer: consumer);

        // hold worker kørende indtil app lukker
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

     // opret user i core api db
    private async Task SyncUserAsync(UserRegisteredEvent userEvent)
    {
        // scope fordi AppDbContext er Scoped, men denne worker er Singleton
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>(); // henter appDbContext fra scope ovenover, ik global DI container.

        // undgå duplikater hvis samme event modtages flere gange
        var UserExists = await context.Users.AnyAsync(u => u.Id == userEvent.UserId);
        if (UserExists)
        {
            _logger.LogInformation("[UserSync] User {UserId} already exists, skipping", userEvent.UserId);
            return;
        }

        // opretter nyt user objekt med samme id som identity service bruger
        context.Users.Add(new User
        {
            Id = userEvent.UserId,
            Name = userEvent.Name,
            Email = userEvent.Email
        });

        // sender SQL insert med EF Core til db
        await context.SaveChangesAsync();
        _logger.LogInformation("[UserSync] Synced user {Name} ({Email}) from Identity", userEvent.Name, userEvent.Email);
    }
}
