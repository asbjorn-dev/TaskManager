using RabbitMQ.Client;

namespace TaskManagement.Api.Infrastructure;

public class RabbitMqConnectionFactory : IDisposable
{
    private readonly IConnection _connection;

    public RabbitMqConnectionFactory(IConfiguration configuration)
    {
        var factory = new ConnectionFactory
        {
            HostName = configuration["RabbitMQ:Host"] ?? "localhost", // hostname = hvor MQ køre, localhost for dev, remote for prod
            UserName = configuration["RabbitMQ:Username"] ?? "guest", // login credentials
            Password = configuration["RabbitMQ:Password"] ?? "guest" // login credentials
        };

         // åbn forbindelse til rabbitMQ server med timeout på 10 sek - hvis rabbit er nede, så hænger app ikke forevigt
        _connection = factory.CreateConnectionAsync().WaitAsync(TimeSpan.FromSeconds(10)).Result;

    }

    public IChannel CreateChannel()
    {
        // opretter channel
        return _connection.CreateChannelAsync().WaitAsync(TimeSpan.FromSeconds(10)).Result;

    }

    public void Dispose()
    {
        _connection?.Dispose();
    }
}
