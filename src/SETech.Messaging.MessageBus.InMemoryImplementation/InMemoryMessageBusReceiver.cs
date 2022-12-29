using SETech.Messaging.MessageBus.Primitives;
using SETech.Messaging.MessageBus.Receiver;

namespace SETech.Messaging.MessageBus.InMemoryImplementation;

public class InMemoryMessageBusReceiver<TPayload> : IMessageBusReceiver<TPayload>
{
    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public ReceiveMode ReceiveMode => throw new NotImplementedException();

    public Task AbandonMessageAsync(ReceivedBusMessage<TPayload> message, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task CompleteMessageAsync(ReceivedBusMessage<TPayload> message, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task DeadLetterMessageAsync(ReceivedBusMessage<TPayload> message, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task DeadLetterMessageAsync
    (
        ReceivedBusMessage<TPayload> message,
        string reason,
        string description,
        CancellationToken cancellationToken = default
    )
    {
        throw new NotImplementedException();
    }

    public Task DeferMessageAsync(ReceivedBusMessage<TPayload> message, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<ReceivedBusMessage<TPayload>> PeekMessageAsync
    (
        long? fromSequenceNumber = null,
        CancellationToken cancellationToken = default
    )
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<ReceivedBusMessage<TPayload>>> PeekMessagesAsync
    (
        int maxMessages,
        long? fromSequenceNumber = null,
        CancellationToken cancellationToken = default
    )
    {
        throw new NotImplementedException();
    }

    public Task<ReceivedBusMessage<TPayload>> ReceiveDeferredMessageAsync
    (
        long sequenceNumber,
        CancellationToken cancellationToken = default
    )
    {
        throw new NotImplementedException();
    }

    public Task<ReceivedBusMessage<TPayload>> ReceiveMessageAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task RenewMessageLockAsync(ReceivedBusMessage<TPayload> message, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
