using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using TaskManagement.Api.Infrastructure;
using TaskManagement.Api.Interfaces;

namespace TaskManagement.Api.Services;

public class RabbitMqPublisher : IEventPublisher, IDisposable
{
    private readonly IChannel _channel;
    private const string ExchangeName = "task-events";

    public RabbitMqPublisher(RabbitMqConnectionFactory connectionFactory)
    {
        // opret kanal til at sende beskeder
        _channel = connectionFactory.CreateChannel();

        // opret exchange til event routing (topic-based, overlever restart)
        // exchange modtager beskeder fra producers og router dem til rigtige queues med rigtige routingKeys
        _channel.ExchangeDeclareAsync(
            exchange: ExchangeName,
            type: ExchangeType.Topic,
            durable: true // beskeder overlever rabbit genstart så dem der er leveret ik går tabt
        ).WaitAsync(TimeSpan.FromSeconds(10)).Wait(); // timeout på 10 sek og returner null hvis den fejler
    }


    public async Task PublishAsync<T>(T eventData, string routingKey)
    {
      var json = JsonSerializer.Serialize(eventData);
      var body = Encoding.UTF8.GetBytes(json);

      await _channel.BasicPublishAsync(
         exchange: ExchangeName,
         routingKey: routingKey,
         body: body
      );

      Console.WriteLine($"[Publisher] Sent {routingKey}: {json}");
    }

    public void Dispose()
    {
        _channel?.Dispose();
    }
}
