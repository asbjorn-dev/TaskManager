using System;

namespace TaskManagement.Api.Interfaces;

public interface IEventPublisher
{
    // en publisher der sender events til notification service. Tager en generic type + 
    // eventdata objekt + routingkey (der bestemmer hvor event skal hen e.g. "task.created") i parametrene 
    Task PublishAsync<T>(T eventData, string routingKey);
}
