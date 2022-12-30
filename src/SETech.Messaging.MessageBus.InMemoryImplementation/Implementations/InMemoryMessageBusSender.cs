using SETech.Messaging.MessageBus.InMemoryImplementation.Bus;
using SETech.Messaging.MessageBus.Primitives;
using SETech.Messaging.MessageBus.Sender;

namespace SETech.Messaging.MessageBus.InMemoryImplementation.Implementations;

/// <summary>The <see cref="InMemoryMessageBusSender{TPayload}"/> sends messages to an in-memory queue or topic.</summary>
/// <inheritdoc />
public class InMemoryMessageBusSender<TPayload> : IMessageBusSender<TPayload>
{
    /// <summary>The <see cref="InMemoryQueue<TPayload>"/> to send messages to.</summary>
    protected InMemoryQueue<TPayload> Queue { get; init; }
    
    public InMemoryMessageBusSender(InMemoryQueue<TPayload> queue)
    {
        Queue = queue;
    }

    public void Dispose() { }

    public Task CancelScheduledMessageAsync(long sequenceNumber, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<long> ScheduleMessageAsync
    (
        BusMessage<TPayload> message,
        DateTimeOffset sendOn,
        CancellationToken cancellationToken = default
    )
    {
        throw new NotImplementedException();
    }

    public Task SendMessageAsync(BusMessage<TPayload> message, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task SendMessagesAsync(IEnumerable<BusMessage<TPayload>> message, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
