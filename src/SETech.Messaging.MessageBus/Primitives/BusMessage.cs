using System.Diagnostics.CodeAnalysis;

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

    /// <summary>The reason this message was dead-lettered, if any.</summary>
    public string? DeadLetterReason { get; init; }

    /// <summary>The description of the reason this message was dead-lettered, if any.</summary>
    public string? DeadLetterDescription { get; init; }

    public BusMessage() { }

    [SetsRequiredMembers]
    public BusMessage
    (
        string? messageId,
        TimeSpan timeToLive,
        TPayload payload,
        string? deadLetterReason = default,
        string? deadLetterDescription = default
    )
    {
        MessageId = messageId;
        TimeToLive = timeToLive;
        Payload = payload;
    }
}
