using System;

namespace TaskManagement.Identity.Interfaces;

public interface IEventPublisher
{
    // sender et event til RabbitMQ - T er event-typen, routingKey bestemmer hvem der modtager det
    Task PublishAsync<T>(T eventData, string routingKey);
}
