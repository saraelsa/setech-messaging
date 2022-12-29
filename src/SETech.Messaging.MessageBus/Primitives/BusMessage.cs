namespace SETech.Messaging.MessageBus.Primitives;

/// <summary>A message that is transferred via a message bus.</summary>
/// <typeparam name="TPayload">The payload type.</typeparam>
public class BusMessage<TPayload>
{
    /// <summary>The ID of the message. This can be any free-form string and can be used for duplicate detection.</summary>
    public string? MessageId { get; init; }

    /// <summary>The duration after which the message expires.</summary>
    public required TimeSpan TimeToLive { get; init; }

    /// <summary>The payload of the message.</summary>
    public required TPayload Payload { get; init; }

    public BusMessage() { }

    public BusMessage(string? messageId, TimeSpan timeToLive, TPayload payload)
    {
        MessageId = messageId;
        TimeToLive = timeToLive;
        Payload = payload;
    }
}
