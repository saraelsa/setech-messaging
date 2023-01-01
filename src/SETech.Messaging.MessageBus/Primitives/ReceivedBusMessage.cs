using System.Diagnostics.CodeAnalysis;

namespace SETech.Messaging.MessageBus.Primitives;

/// <summary>A message that was received by a <see cref="Receiver.IMessageBusReceiver{TPayload}"/>.</summary>
/// <typeparam name="TPayload">The payload type.</typeparam>
public class ReceivedBusMessage<TPayload> : BusMessage<TPayload>
{
    /// <summary>The sequence number of this message.</summary>
    public required long SequenceNumber { get; init; }

    /// <summary>The timestamp at which the message was sent.</summary>
    public required DateTimeOffset Timestamp { get; init; }

    /// <summary>Whether this message is deferred.</summary>
    public required bool Deferred { get; init; } = false;

    /// <summary>The time this message is scheduled to be sent at, or null if it is not currently scheduled.</summary>
    public required DateTimeOffset? ScheduledFor { get; init; } = null;

    public ReceivedBusMessage() { }

    [SetsRequiredMembers]
    public ReceivedBusMessage
    (
        string? messageId,
        TimeSpan timeToLive,
        TPayload payload,
        long sequenceNumber,
        DateTimeOffset timestamp,
        bool deferred,
        DateTimeOffset? scheduledFor
    )
        : base(messageId, timeToLive, payload)
    {
        SequenceNumber = sequenceNumber;
        Timestamp = timestamp;
        Deferred = deferred;
        ScheduledFor = scheduledFor;
    }
}
