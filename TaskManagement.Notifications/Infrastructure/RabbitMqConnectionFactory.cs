using System;
using RabbitMQ.Client;

namespace TaskManagement.Notifications.Infrastructure;

// håndterer RabbitMQ connection (samme pattern som Core API)
public class RabbitMqConnectionFactory : IAsyncDisposable
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

        // Opret forbindelse til rabbit med 10 sek timeout
        _connection = factory.CreateConnectionAsync()
            .WaitAsync(TimeSpan.FromSeconds(10)).Result;
    }

    // opret kanal til lytte på beskeder
    public async Task<IChannel> CreateChannelAsync()
    {
        return await _connection.CreateChannelAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await _connection.DisposeAsync();
    }
}
