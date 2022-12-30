using SETech.Messaging.MessageBus.Primitives;

namespace SETech.Messaging.MessageBus.Receiver;

/// <summary>The <see cref="IMessageBusReceiver"/> receives messages from a queue or a subscription to a topic.</summary>
/// <typeparam name="TPayload">The payload type to receive.</typeparam>
public interface IMessageBusReceiver<TPayload> : IDisposable
{
    /// <summary>The mode to receive messages with. This controls how messages are settled.</summary>
    public ReceiveMode ReceiveMode { get; }

    /// <summary>Receives the next message in the queue or subscription.</summary>
    /// <remarks>If there is no message in the queue or subscription, this method waits until a message arrives.</remarks>
    /// <param name="cancellationToken">
    ///     A <see cref="CancellationToken"/> to observe while receiving the next message.
    /// </param>
    /// <returns>The <see cref="ReceivedBusMessage{TPayload}"/> that has been received.</returns>
    public Task<ReceivedBusMessage<TPayload>> ReceiveMessageAsync(CancellationToken cancellationToken = default);

    /// <summary>Renews the lock on a message.</summary>
    /// <remarks>This method is only valid if the receive mode is <see cref="ReceiveMode.PeekLock"/>.</remarks>
    /// <param name="message">The <see cref="ReceivedBusMessage{TPayload}"/> whose lock should be renewed.</param>
    /// <param name="cancellationToken">
    ///     A <see cref="CancellationToken"/> to observe while renewing the lock on the message.
    /// </param>
    /// <exception cref="LockExpiredException">Thrown when the lock on the message has already expired.</exception>
    /// <exception cref="NotSupportedException">
    ///     Thrown when the receive mode is not <see cref="ReceiveMode.PeekLock"/>.
    /// </exception>
    public Task RenewMessageLockAsync(ReceivedBusMessage<TPayload> message, CancellationToken cancellationToken = default);

    /// <summary>Settles a received message.</summary>
    /// <remarks>This method is only valid if the receive mode is <see cref="ReceiveMode.PeekLock"/>.</remarks>
    /// <param name="message">The <see cref="ReceivedBusMessage{TPayload}"/> to mark as complete.</param>
    /// <param name="cancellationToken">
    ///     A <see cref="CancellationToken"/> to observe while marking the message as complete.
    /// </param>
    /// <exception cref="LockExpiredException">Thrown when the lock on the message has already expired.</exception>
    /// <exception cref="NotSupportedException">
    ///     Thrown when the receive mode is not <see cref="ReceiveMode.PeekLock"/>.
    /// </exception>
    public Task CompleteMessageAsync(ReceivedBusMessage<TPayload> message, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Receives a message in the queue or subscription without locking it. This message is open to being locked and
    ///     settled by this or another application. The first call returns the first message in the queue. Each subsequent
    ///     call receives the next message.
    /// </summary>
    /// <remarks>This method may return deferred messages.</remarks>
    /// <param name="fromSequenceNumber">
    ///     The minimum sequence number of the message to receive. If this is set, the peek pointer will be reset.
    /// </param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while peeking the next message.</param>
    /// <returns>The <see cref="ReceivedBusMessage{TPayload}"/> that has been received, if any.</returns>
    public Task<ReceivedBusMessage<TPayload>?> PeekMessageAsync
    (
        long? fromSequenceNumber = default,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    ///     Receives a number of messages in the queue or subscription without locking them. The messages are open to being
    ///     locked and settled by this or another application.
    /// </summary>
    /// <remarks>This method may return deferred messages.</remarks>
    /// <param name="maxMessages">The maximum number of messages to receive.</param>
    /// <param name="fromSequenceNumber">The minimum sequence number of the messages to receive.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while peeking the next message.</param>
    /// <returns>
    ///     The <see cref="IReadOnlyCollection{ReceivedBusMessage{TPayload}}"/> of messages that have been received.
    /// </returns>
    public Task<IReadOnlyCollection<ReceivedBusMessage<TPayload>>> PeekMessagesAsync
    (
        int maxMessages,
        long? fromSequenceNumber = default,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    ///     Releases a received message without marking it as complete.
    ///     The message may be received by this or other receivers.
    /// </summary>
    /// <remarks>This method is only valid if the receive mode is <see cref="ReceiveMode.PeekLock"/>.</remarks>
    /// <param name="message">The <see cref="ReceivedBusMessage{TPayload}"/> to abandon.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while abandoning the message.</param>
    /// <exception cref="LockExpiredException">Thrown when the lock on the message has already expired.</exception>
    /// <exception cref="NotSupportedException">
    ///     Thrown when the receive mode is not <see cref="ReceiveMode.PeekLock"/>.
    /// </exception>
    public Task AbandonMessageAsync(ReceivedBusMessage<TPayload> message, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Defers a message to be received later with the <see cref="ReceiveDeferredMessageAsync"/> method using its
    ///     sequence number.
    /// </summary>
    /// <remarks>This method is only valid if the receive mode is <see cref="ReceiveMode.PeekLock"/>.</remarks>
    /// <param name="message">The <see cref="ReceivedBusMessage{TPayload}"/> to defer.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while deferring the message.</param>
    /// <exception cref="LockExpiredException">Thrown when the lock on the message has already expired.</exception>
    /// <exception cref="NotSupportedException">
    ///     Thrown when the receive mode is not <see cref="ReceiveMode.PeekLock"/>.
    /// </exception>
    public Task DeferMessageAsync(ReceivedBusMessage<TPayload> message, CancellationToken cancellationToken = default);

    /// <summary>Receives a deferred message in the queue or subscription.</summary>
    /// <param name="sequenceNumber">The sequence number of the deferred message to receive.</param>
    /// <param name="cancellationToken">
    ///     A <see cref="CancellationToken"/> to observe while receiving the deferred message.
    /// </param>
    /// <exception cref="MessageNotFoundException">
    ///     Thrown when the message with the specified sequence number is not found.
    /// </exception>
    /// <returns>The <see cref="ReceivedBusMessage{TPayload}"/> that has been received.</returns>
    public Task<ReceivedBusMessage<TPayload>> ReceiveDeferredMessageAsync
    (
        long sequenceNumber,
        CancellationToken cancellationToken = default
    );

    /// <summary>Moves a message to the dead-letter queue.</summary>
    /// <remarks>This method is only valid if the receive mode is <see cref="ReceiveMode.PeekLock"/>.</remarks>
    /// <param name="message">The <see cref="ReceivedBusMessage{TPayload}"/> to move to the dead-letter queue.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while deferring the message.</param>
    /// <exception cref="LockExpiredException">Thrown when the lock on the message has already expired.</exception>
    /// <exception cref="NotSupportedException">
    ///     Thrown when the receive mode is not <see cref="ReceiveMode.PeekLock"/>.
    /// </exception>
    public Task DeadLetterMessageAsync(ReceivedBusMessage<TPayload> message, CancellationToken cancellationToken = default);

    /// <summary>Moves a message to the dead-letter queue.</summary>
    /// <remarks>This method is only valid if the receive mode is <see cref="ReceiveMode.PeekLock"/>.</remarks>
    /// <param name="message">The <see cref="ReceivedBusMessage{TPayload}"/> to move to the dead-letter queue.</param>
    /// <param name="reason">The reason the message was moved to the dead-letter queue.</param>
    /// <param name="description">A description on the reason the message was moved to the dead-letter queue.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while deferring the message.</param>
    /// <exception cref="LockExpiredException">Thrown when the lock on the message has already expired.</exception>
    /// <exception cref="NotSupportedException">
    ///     Thrown when the receive mode is not <see cref="ReceiveMode.PeekLock"/>.
    /// </exception>
    public Task DeadLetterMessageAsync
    (
        ReceivedBusMessage<TPayload> message,
        string reason,
        string description,
        CancellationToken cancellationToken = default
    );
}
