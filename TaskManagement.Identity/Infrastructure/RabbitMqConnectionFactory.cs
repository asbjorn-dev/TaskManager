using System;
using RabbitMQ.Client;

namespace TaskManagement.Identity.Infrastructure;

// styre forbindelsen til rabbitMQ
public class RabbitMqConnectionFactory : IDisposable
{
    private readonly IConnection _connection;
    public RabbitMqConnectionFactory(IConfiguration configuration)
    {
        var factory = new ConnectionFactory
        {
            HostName = configuration["RabbitMQ:Host"] ?? "localhost",
            UserName = configuration["RabbitMQ:Username"] ?? "guest",
            Password = configuration["RabbitMQ:Password"] ?? "guest"
        };

        // åbn forbindelse med timeout - hvis RabbitMQ er nede hænger appen ikke for evigt
        _connection = factory.CreateConnectionAsync().WaitAsync(TimeSpan.FromSeconds(10)).Result;
    }

    public IChannel CreateChannel()
    {
        return _connection.CreateChannelAsync().WaitAsync(TimeSpan.FromSeconds(10)).Result;
    }

    public void Dispose()
    {
        _connection?.Dispose();
    }

}
