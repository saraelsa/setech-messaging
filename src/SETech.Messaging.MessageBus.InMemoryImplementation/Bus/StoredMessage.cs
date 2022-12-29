using System.Diagnostics.CodeAnalysis;
using SETech.Messaging.MessageBus.Primitives;

namespace SETech.Messaging.MessageBus.InMemoryImplementation.Bus;

public class StoredMessage<TPayload> : BusMessage<TPayload>
{
    public long SequenceNumber { get; init; }

    public DateTimeOffset Timestamp { get; protected init; } = DateTimeOffset.Now;

    public int DeliveryAttempts { get; set; } = 0;

    [SetsRequiredMembers]
    public StoredMessage(BusMessage<TPayload> message, long sequenceNumber)
        : base(message.MessageId, message.TimeToLive, message.Payload)
    {
        SequenceNumber = sequenceNumber;
    }
}
