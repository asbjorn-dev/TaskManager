using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using TaskManagement.Notifications.Handlers;
using TaskManagement.Notifications.Infrastructure;

namespace TaskManagement.Notifications;


// worker kender ik til specifikke events, den finder handler via IeventHandler.RoutingKey.
// betyder hvis ny event --> opret ny handler og implementer kun der (Open/closed principle i solid)
public class Worker : BackgroundService // backgroundservice køre heletiden (lytter til rabbit)
{
    private readonly ILogger<Worker> _logger;
    private readonly RabbitMqConnectionFactory _connectionFactory;
    private readonly IEnumerable<IEventHandler> _handlers; // alle handlers injected her
    private IChannel? _channel;
    private const string ExchangeName = "task-events";
    private const string QueueName = "notification-queue";

    public Worker(
        ILogger<Worker> logger,
        RabbitMqConnectionFactory connectionFactory,
        IEnumerable<IEventHandler> handlers)
    {
        _logger = logger;
        _connectionFactory = connectionFactory;
        _handlers = handlers;
    }


    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // opret channel til Rabbit
        _channel = await _connectionFactory.CreateChannelAsync();

        // opret exchange til event routing (topic-based, overlever restart)
        // exchange modtager beskeder fra producers og router dem til rigtige queues med rigtige routingKeys
        await _channel.ExchangeDeclareAsync(
            exchange: ExchangeName,
            type: ExchangeType.Topic,
            durable: true
        );

        // opret kø til at holde beskeder
        await _channel.QueueDeclareAsync(
            queue: QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false
        );

        // bind kø til exchange med "task.*" (modtager task.created etc.)
        await _channel.QueueBindAsync(
            queue: QueueName,
            exchange: ExchangeName,
            routingKey: "task.*" // lytter på alt der starter med "task."
        );

        _logger.LogInformation("[Notification Service] Waiting for messages....");

        // opret consumer der lytter
        var consumer = new AsyncEventingBasicConsumer(_channel);

        // besked modtages --> kør denne kode
        consumer.ReceivedAsync += async (model, ea) =>
        {
            // convert besked til string
            var json = Encoding.UTF8.GetString(ea.Body.ToArray());
            var routingKey = ea.RoutingKey;

            // find den rigtige handler der matcher routingKey fra producer
            var handler = _handlers.FirstOrDefault(h => h.RoutingKey == routingKey);

            if (handler != null)
                await handler.HandleAsync(json); // kalder den specifikke handler
            else
                _logger.LogWarning("[Notification] No handler for: {routingKey}", routingKey);
            
            // bekræfter beskeden er behandlet (fjerner fra køen)
            await _channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
        };

        // consumer (lytter på kø)
        await _channel.BasicConsumeAsync(
            queue: QueueName,
            autoAck: false,
            consumer: consumer
        );

        // hold service kørende indtil den stoppes
        while (!stoppingToken.IsCancellationRequested)
            await Task.Delay(1000, stoppingToken);
    }

    // cleanup når service stopper
    public override void Dispose()
    {
        _channel?.Dispose();
        base.Dispose();
    }
}
