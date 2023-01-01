using SETech.Messaging.MessageBus.InMemoryImplementation.Bus;
using SETech.Messaging.MessageBus.Primitives;
using SETech.Messaging.MessageBus.Receiver;

namespace SETech.Messaging.MessageBus.InMemoryImplementation.Implementations;

/// <summary>
///     The <see cref="InMemoryMessageBusReceiver{TPayload}"/> receives messages from an in-memory queue or subscription to
///     a topic.
/// </summary>
/// <inheritdoc />
public class InMemoryMessageBusReceiver<TPayload> : IMessageBusReceiver<TPayload>
{
    /// <summary>The <see cref="InMemoryQueue<TPayload>"/> to receive messages from.</summary>
    protected InMemoryQueue<TPayload> Queue { get; init; }

    /// <summary>The <see cref="ReceivedMessageActions"/> for received messages stored for later usage.</summary>
    /// <remarks>These are only stored when <see cref="ReceiveMode"/> is <see cref="ReceiveMode.PeekLock"/>.</remarks>
    protected IDictionary<long, ReceivedMessageActions> InProcessMessageActions { get; } =
        new SortedDictionary<long, ReceivedMessageActions>();

    /// <summary>
    ///     The current pointer used for sequential peeking. The next peeked message with an implied minimum sequence number
    ///     will use this value as the minimum sequence number.
    /// </summary>
    protected long PeekPointer { get; } = 0;

    /// <summary>Creates an <see cref="InMemoryMessageBusReceiver{TPayload}"/>.</summary>
    /// <param name="queue">The queue to receive messages from.</param>
    /// <param name="options">Configurable options for the receiver.</param>
    public InMemoryMessageBusReceiver(InMemoryQueue<TPayload> queue, ReceiverOptions options)
    {
        Queue =
            options.SubQueue switch
            {
                SubQueue.None => queue,
                SubQueue.DeadLetter => queue.DeadLetterQueue!,
                _ => throw new NotSupportedException()
            };

        ReceiveMode = options.ReceiveMode;
    }

    public void Dispose() { }

    public ReceiveMode ReceiveMode { get; protected init; }

    public Task<ReceivedBusMessage<TPayload>?> PeekMessageAsync
    (
        long? fromSequenceNumber = null,
        CancellationToken cancellationToken = default
    ) => Task.FromResult(Queue.Peek(fromSequenceNumber ?? PeekPointer));

    public Task<IReadOnlyCollection<ReceivedBusMessage<TPayload>>> PeekMessagesAsync
    (
        int maxMessages,
        long? fromSequenceNumber = null,
        CancellationToken cancellationToken = default
    ) => Task.FromResult(Queue.PeekMany(fromSequenceNumber ?? 0, maxMessages).AsReadOnly()
            as IReadOnlyCollection<ReceivedBusMessage<TPayload>>);

    public Task<ReceivedBusMessage<TPayload>> ReceiveMessageAsync(CancellationToken cancellationToken = default)
    {
        TaskCompletionSource<ReceivedBusMessage<TPayload>> taskCompletionSource = new (cancellationToken);

        Queue.Receive((message, actions) =>
        {
            HandleReceivedMessage(message, actions);
            taskCompletionSource.SetResult(message);
        });

        return taskCompletionSource.Task;
    }

    /// <summary>Handles a received message.</summary>
    /// <param name="message">The message that was received.</param>
    /// <param name="actions">The actions that can be used to settle the message.</param>
    protected void HandleReceivedMessage(ReceivedBusMessage<TPayload> message, ReceivedMessageActions actions)
    {
        switch (ReceiveMode)
        {
            case ReceiveMode.PeekLock:
                InProcessMessageActions[message.SequenceNumber] = actions;
                break;

            case ReceiveMode.ReceiveAndDelete:
                actions.Complete();
                break;
        }
    }

    /// <summary>Gets the <see cref="ReceivedMessageActions"/> for a previously received message.</summary>
    /// <param name="sequenceNumber">The sequence number of the message to get the actions for.s</param>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when <see cref="ReceiveMode"/> is not <see cref="ReceiveMode.PeekLock"/>. Message actions are only stored
    ///     for later usage in this mode.
    /// </exception>
    /// <exception cref="LockExpiredException">
    ///     Thrown when the requested message actions do not exist. <see cref="LockExpiredException"/> is thrown instead of
    ///     <see cref="MessageNotFoundException"/> because this method is intended as a guard on message settlement methods.
    ///     In their context, if the message does not exist in the receiver's store of message actions, it implies that the
    ///     message has already been settled, which means that the lock on the message is no longer valid.
    /// </exception>
    protected ReceivedMessageActions GetMessageActionsOrThrow(long sequenceNumber)
    {
        if (ReceiveMode != ReceiveMode.PeekLock)
            throw new InvalidOperationException("This operation is only valid in the PeekLock receive mode.");

        ReceivedMessageActions? actions;

        if (!InProcessMessageActions.TryGetValue(sequenceNumber, out actions))
            throw new LockExpiredException();

        InProcessMessageActions.Remove(sequenceNumber);

        return actions;
    }

    public Task RenewMessageLockAsync(ReceivedBusMessage<TPayload> message, CancellationToken cancellationToken = default)
    {
        GetMessageActionsOrThrow(message.SequenceNumber).RenewLock();

        return Task.CompletedTask;
    }

    public Task CompleteMessageAsync(ReceivedBusMessage<TPayload> message, CancellationToken cancellationToken = default)
    {
        GetMessageActionsOrThrow(message.SequenceNumber).Complete();

        return Task.CompletedTask;
    }

    public Task AbandonMessageAsync(ReceivedBusMessage<TPayload> message, CancellationToken cancellationToken = default)
    {
        GetMessageActionsOrThrow(message.SequenceNumber).Complete();

        return Task.CompletedTask;
    }

    public Task DeadLetterMessageAsync
    (
        ReceivedBusMessage<TPayload> message,
        CancellationToken cancellationToken = default
    ) => DeadLetterMessageAsync(message, reason: null, description: null, cancellationToken);

    public Task DeadLetterMessageAsync
    (
        ReceivedBusMessage<TPayload> message,
        string? reason,
        string? description,
        CancellationToken cancellationToken = default
    )
    {
        GetMessageActionsOrThrow(message.SequenceNumber).DeadLetter(reason, description);

        return Task.CompletedTask;
    }

    public Task DeferMessageAsync(ReceivedBusMessage<TPayload> message, CancellationToken cancellationToken = default)
    {
        GetMessageActionsOrThrow(message.SequenceNumber).Defer();

        return Task.CompletedTask;
    }

    public Task<ReceivedBusMessage<TPayload>> ReceiveDeferredMessageAsync
    (
        long sequenceNumber,
        CancellationToken cancellationToken = default
    )
    {
        throw new NotImplementedException();
    }
}
