namespace SETech.Messaging.MessageBus.Primitives;

/// <summary>A message that is transferred via a message bus.</summary>
/// <typeparam name="TPayload">The payload type.</typeparam>
public class BusMessage<TPayload>
{
    /// <summary>The ID of the message. This can be any free-form string and can be used for duplicate detection.</summary>
    public string? MessageId { get; init; }

    /// <summary>
    ///     The correlation ID of the message. This can be used for two-way communication using
    ///     <see cref="Receiver.IMessageBusReceiver{TPayload}"/>.
    /// </summary>
    public string? CorrelationId { get; init; }

    /// <summary>The duration after which the message expires.</summary>
    public required TimeSpan TimeToLive { get; init; }

    /// <summary>The payload of the message.</summary>
    public required TPayload Payload { get; init; }

    public BusMessage() { }

    public BusMessage(string? messageId, string? correlationId, TimeSpan timeToLive, TPayload payload)
    {
        MessageId = messageId;
        CorrelationId = correlationId;
        TimeToLive = timeToLive;
        Payload = payload;
    }
}
