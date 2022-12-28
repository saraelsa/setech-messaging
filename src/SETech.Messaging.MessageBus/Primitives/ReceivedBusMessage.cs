namespace SETech.Messaging.MessageBus.Primitives;

/// <summary>A message that was received by a <see cref="Receiver.IMessageBusReceiver{TPayload}"/>.</summary>
/// <typeparam name="TPayload">The payload type.</typeparam>
public class ReceivedBusMessage<TPayload> : BusMessage<TPayload>
{
    /// <summary>The sequence number of this message.</summary>
    public required long SequenceNumber { get; init; }

    public ReceivedBusMessage() { }

    public ReceivedBusMessage
    (
        string? messageId,
        string? correlationId,
        TimeSpan timeToLive,
        TPayload payload,
        long sequenceNumber
    )
        : base(messageId, correlationId, timeToLive, payload)
    {
        SequenceNumber = sequenceNumber;
    }
}
