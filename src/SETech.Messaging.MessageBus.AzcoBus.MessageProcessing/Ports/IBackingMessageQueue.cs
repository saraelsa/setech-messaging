using SETech.Messaging.MessageBus.AzcoBus.MessageProcessing.Internal;

namespace SETech.Messaging.MessageBus.AzcoBus.MessageProcessing.Ports;

/// <summary>The port for a backing message queue that will store messages and allow their retreival.</summary>
public interface IBackingMessageQueue
{
    /// <summary>Allocates a sequence number that can be used to enqueue the next message.</summary>
    public Task<long> AllocateSequenceNumber(CancellationToken cancellationToken = default);

    /// <summary>Updates a message in this queue.</summary>
    /// <param name="message">The message to update, matched with its sequence number.</param>
    /// <param name="cancellationToken">The cancellation token to observe while updating the message.</param>
    public Task UpdateMessage(StoredMessage message, CancellationToken cancellationToken = default);

    /// <summary>Enqueues a message into this queue.</summary>
    /// <param name="message">The message to enqueue.</param>
    /// <param name="cancellationToken">The cancellation token to observe while enqueuing the message.</param>
    public Task EnqueueMessage(StoredMessage message, CancellationToken cancellationToken = default);

    /// <summary>Tries to dequeue a message from this queue.</summary>
    /// <param name="cancellationToken">The cancellation token to observe while dequeuing the message.</param>
    /// <returns>The dequeued message, or null if there was no message to dequeue.</returns>
    public Task<StoredMessage?> DequeueMessage(CancellationToken cancellationToken = default);

    /// <summary>Enqueues a message into the deferred messages queue.</summary>
    /// <param name="message">The message to enqueue.</param>
    public Task EnqueueDeferredMessage(StoredMessage message, CancellationToken cancellationToken = default);

    /// <summary>Tries to dequeue a specific message from the deferred messages queue.</summary>
    /// <param name="sequenceNumber">The sequence number of the message to dequeue.</param>
    /// <param name="cancellationToken">The cancellation token to observe while dequeuing the message.</param>
    /// <returns>The dequeued message, or null if there was no message to dequeue.</returns>
    public Task<StoredMessage?> DequeueDeferredMessage(long sequenceNumber, CancellationToken cancellationToken = default);

    /// <summary>Enqueues a message into the scheduled messages queue.</summary>
    /// <remarks>
    ///     The message is enqueued to a position based on an ascending <see cref="StoredMessage.ScheduledEnqueueTimeUtc"/>
    ///     order.
    /// </remarks>
    /// <param name="message">The message to enqueue.</param>
    /// <param name="cancellationToken">The cancellation token to observe while enqueuing the message.</param>
    public Task EnqueueScheduledMessage(StoredMessage message, CancellationToken cancellationToken = default);

    /// <summary>Peeks a message from the scheduled messages queue without dequeuing it.</summary>
    /// <remarks>The scheduled message with the earliest scheduled time is peeked.</remarks>
    /// <param name="cancellationToken">The cancellation token to observe while peeking the message.</param>
    public Task<StoredMessage?> PeekScheduledMessage(CancellationToken cancellationToken = default);

    /// <summary>Dequeues a message from the scheduled messages queue and enqueues it into the main queue.</summary>
    /// <remarks>The scheduled message with the earliest scheduled time is transferred. The operation is atomic.</remarks>
    /// <param name="cancellationToken">The cancellation token to observe while transferring the message.</param>
    public Task<StoredMessage?> TransferScheduledMessage(CancellationToken cancellationToken = default);

    /// <summary>Peeks a message at without dequeuing it.</summary>
    /// <remarks>To include deferred messages, use <see cref="DiagnosticPeekMessage"/>.</remarks>
    /// <param name="cancellationToken">The cancellation token to observe while peeking the message.</param>
    /// <returns>The peeked message, or null if there was no message to peek.</returns>
    public Task<StoredMessage?> PeekMessage(CancellationToken cancellationToken = default);

    /// <summary>Peeks a message at without dequeuing it.</summary>
    /// <remarks>
    ///     Messages are included in this order: deferred messages, followed by normal messages, followed by scheduled messages
    ///     in ascending order of scheduled publication time.
    /// </remarks>
    /// <param name="fromSequenceNumber">The sequence number to start peeking from.</param>
    /// <param name="cancellationToken">The cancellation token to observe while peeking the message.</param>
    /// <returns>The peeked message, or null if there was no message to peek.</returns>
    public Task<StoredMessage?> DiagnosticPeekMessage(long fromSequenceNumber, CancellationToken cancellationToken = default);

    /// <summary>Peeks a list of messages at the beginning of this queue without dequeuing them.</summary>
    /// <remarks>
    ///     Messages are included in this order: deferred messages, followed by normal messages, followed by scheduled messages
    ///     in ascending order of scheduled publication time.
    /// </remarks>
    /// <param name="fromSequenceNumber">The sequence number to start peeking from.</param>
    /// <param name="maximumMessages">The maximum number of messages to peek.</param>
    /// <param name="cancellationToken">The cancellation token to observe while peeking the messages.</param>
    /// <returns>The peeked messages.</returns>
    public Task<ICollection<StoredMessage>> DiagnosticPeekMessages(
        long fromSequenceNumber,
        int maximumMessages,
        CancellationToken cancellationToken = default);
}
