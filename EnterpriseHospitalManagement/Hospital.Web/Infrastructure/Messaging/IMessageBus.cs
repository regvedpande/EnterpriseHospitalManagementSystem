using Hospital.Web.Infrastructure.Messaging.Messages;

namespace Hospital.Web.Infrastructure.Messaging;

/// <summary>
/// Abstraction over a message broker (RabbitMQ, Azure Service Bus, etc.).
/// </summary>
public interface IMessageBus
{
    /// <summary>Publish a domain event to the named exchange/topic.</summary>
    Task PublishAsync<T>(string routingKey, T message, CancellationToken ct = default)
        where T : HmsMessage;

    /// <summary>Whether the underlying broker connection is available.</summary>
    bool IsConnected { get; }
}
