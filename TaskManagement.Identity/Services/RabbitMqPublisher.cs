using System;
using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using TaskManagement.Identity.Infrastructure;
using TaskManagement.Identity.Interfaces;

namespace TaskManagement.Identity.Services;

public class RabbitMqPublisher : IEventPublisher, IDisposable
{
    private readonly IChannel _channel;
    private readonly ILogger<RabbitMqPublisher> _logger;
    private const string ExchangeName = "user-events"; // user exchange i rabbit for user events
    public RabbitMqPublisher(RabbitMqConnectionFactory connectionFactory, ILogger<RabbitMqPublisher> logger)
    {
        _logger = logger;
        _channel = connectionFactory.CreateChannel();

        // declare exchange så den eksistere når vi publisher - overlever rabbit genstart
        _channel.ExchangeDeclareAsync(
            exchange: ExchangeName,
            type: ExchangeType.Topic,
            durable: true
        ).WaitAsync(TimeSpan.FromSeconds(10)).Wait();
    }

    public async Task PublishAsync<T>(T eventData, string routingKey)
    {
        // convert c# objekt (f.eks. UserRegisteredEvent) til JSON
        var json = JsonSerializer.Serialize(eventData);
        var body = Encoding.UTF8.GetBytes(json); // rabbit arbejder med rå bytes

        // send til rabbit - channel er forbindelsen og BasicPublishAsync() sender beskeden
        await _channel.BasicPublishAsync(
            exchange: ExchangeName,
            routingKey: routingKey,
            body: body
        );

        _logger.LogInformation("[Publisher] Sent {RoutingKey}: {Json}", routingKey, json);
    }

    public void Dispose()
    {
        _channel?.Dispose();
    }
}
