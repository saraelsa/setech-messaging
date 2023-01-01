using System.Diagnostics.CodeAnalysis;
using SETech.Messaging.MessageBus.Primitives;

namespace SETech.Messaging.MessageBus.InMemoryImplementation.Bus;

/// <summary>Represents a message that is stored in-memory.</summary>
public class StoredMessage<TPayload> : BusMessage<TPayload>
{
    /// <summary>The sequence number of the message.</summary>
    public long SequenceNumber { get; init; }

    /// <summary>The timestamp at which the message was sent.</summary>
    public DateTimeOffset Timestamp { get; protected init; } = DateTimeOffset.Now;

    /// <summary>The number of times the message was unsuccesfully attempted to be delivered.</summary>
    public int DeliveryAttempts { get; set; } = 0;

    /// <summary>Whether this message is deferred.</summary>
    public bool Deferred { get; set; } = false;

    /// <summary>Whether this message is locked.</summary>
    public bool Locked { get; set; } = false;

    /// <summary>The time this message is scheduled to be sent at, or null if it is not currently scheduled.</summary>
    public DateTimeOffset? ScheduledFor { get; set; } = null;

    /// <summary>Creates a <see cref="StoredMessage{TPayload}"/>.</summary>
    /// <param name="message">
    ///     The <see cref="BusMessage{TPayload}"/> to create this <see cref="StoredMessage{TPayload}"/> from.
    /// </param>
    /// <param name="sequenceNumber">The sequence number of the message.</param>
    [SetsRequiredMembers]
    public StoredMessage(BusMessage<TPayload> message, long sequenceNumber)
        : base(message.MessageId, message.TimeToLive, message.Payload)
    {
        SequenceNumber = sequenceNumber;
    }
}
