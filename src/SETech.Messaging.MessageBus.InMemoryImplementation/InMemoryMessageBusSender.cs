using SETech.Messaging.MessageBus.Primitives;
using SETech.Messaging.MessageBus.Sender;

namespace SETech.Messaging.MessageBus.InMemoryImplementation;

public class InMemoryMessageBusSender<TPayload> : IMessageBusSender<TPayload>
{
    public void Dispose()
    {
        throw new NotImplementedException();
    }

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
