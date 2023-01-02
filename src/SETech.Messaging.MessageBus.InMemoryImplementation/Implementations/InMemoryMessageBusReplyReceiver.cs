using SETech.Messaging.MessageBus.InMemoryImplementation.Bus;
using SETech.Messaging.MessageBus.Primitives;
using SETech.Messaging.MessageBus.Receiver;

namespace SETech.Messaging.MessageBus.InMemoryImplementation.Implementations;

/// <summary>The <see cref="InMemoryMessageBusReplyReceiver{TPayload}"/> receives replies from an in-memory queue.</summary>
/// <inheritdoc />
public class InMemoryMessageBusReplyReceiver<TPayload> : IMessageBusReplyReceiver<TPayload>
    where TPayload : ICorrelatedMessagePayload
{
    /// <summary>The <see cref="InMemoryQueue<TPayload>"/> to receive messages from.</summary>
    protected InMemoryQueue<TPayload> Queue { get; init; }

    /// <summary>Creates an <see cref="InMemoryMessageBusReplyReceiver{TPayload}"/>.</summary>
    /// <param name="queue">The queue to receive replies from.</param>
    public InMemoryMessageBusReplyReceiver(InMemoryQueue<TPayload> queue)
    {
        Queue = queue;
    }

    public void Dispose() { }

    public Task<ReceivedBusMessage<TPayload>> ReceiveReplyAsync
    (
        string correlationId,
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        TaskCompletionSource<ReceivedBusMessage<TPayload>> tcs = new (cancellationToken);

        ReceiveReplyAsTaskResult(correlationId, tcs, cancellationToken);

        return tcs.Task;
    }

    protected void ReceiveReplyAsTaskResult
    (
        string correlationId,
        TaskCompletionSource<ReceivedBusMessage<TPayload>> tcs,
        CancellationToken cancellationToken
    )
    {
        long? existingReplySequenceNumber = TryFindDeferredReplySequenceNumber(correlationId);

        if (existingReplySequenceNumber is not null)
        {
            Queue.ReceiveDeferred(existingReplySequenceNumber.Value, (message, actions) =>
            {
                if (tcs.TrySetResult(message))
                    actions.Complete();
                else
                    actions.Abandon();
            });
        }
        else
        {
            ReceiveNonDeferredReplyAsTaskResult(correlationId, tcs, cancellationToken);
        }
    }

    protected void ReceiveNonDeferredReplyAsTaskResult
    (
        string correlationId,
        TaskCompletionSource<ReceivedBusMessage<TPayload>> tcs,
        CancellationToken cancellationToken
    )
    {
        Queue.Receive((message, actions) =>
        {
            if (cancellationToken.IsCancellationRequested)
            {
                actions.Abandon();
                tcs.SetCanceled(cancellationToken);

                return;
            }

            if (message.Payload.CorrelationId == correlationId)
            {
                if (tcs.TrySetResult(message))
                    actions.Complete();
                else
                    actions.Abandon();

                return;
            }

            actions.Abandon();

            // If the message is still in the queue, it means that its receiver has not requested it yet.
            //
            // It will be deferred so that when the receiver requests it, it can peek through the existing messages and find
            // it.
            //
            // We can not simply abandon it; otherwise, we will be stuck in an infinite look with this reply receiver
            // processing and abandoning it over and over again.
            if (Queue.Peek(fromSequenceNumber: 0)?.SequenceNumber == message.SequenceNumber)
            {
                Queue.Receive((message, actions) =>
                {
                    actions.Defer();
                });
            }

            ReceiveNonDeferredReplyAsTaskResult(correlationId, tcs, cancellationToken);
        });
    }

    protected long? TryFindDeferredReplySequenceNumber(string correlationId)
    {
        int currentSequenceNumber = 0;

        while (true)
        {
            ReceivedBusMessage<TPayload>? receivedMessage = Queue.Peek(fromSequenceNumber: currentSequenceNumber++);

            if (receivedMessage is null)
                break;
            
            if (receivedMessage.Deferred)
            {
                if (receivedMessage.Payload.CorrelationId == correlationId)
                    return receivedMessage.SequenceNumber;

                CheckDeferredMessageForExpiry(receivedMessage);
            }
        }

        return null;
    }

    protected void CheckDeferredMessageForExpiry(ReceivedBusMessage<TPayload> message)
    {
        if (message.Timestamp + message.TimeToLive > DateTime.Now)
        {
            Queue.ReceiveDeferred(message.SequenceNumber, (message, actions) =>
            {
                actions.DeadLetter("TTLExpiredException", null);
            });
        }
    }
}
