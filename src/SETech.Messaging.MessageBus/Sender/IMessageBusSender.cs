using SETech.Messaging.MessageBus.Primitives;

namespace SETech.Messaging.MessageBus.Sender;

/// <summary>The <see cref="IMessageBusSender"/> publishes messages to a queue or a topic.</summary>
/// <typeparam name="TPayload">The payload type to send.</typeparam>
public interface IMessageBusSender<TPayload>
{
    /// <summary>Sends a message to the queue or subscription.</summary>
    /// <param name="message">The <see cref="BusMessage{TPayload}"/> to send.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while sending the message.</param>
    public Task SendMessageAsync(BusMessage<TPayload> message, CancellationToken cancellationToken = default);

    /// <summary>Sends multiple messages to the queue or subscription.</summary>
    /// <param name="messages">The messages to send.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while sending the message.</param>
    public Task SendMessagesAsync(IEnumerable<BusMessage<TPayload>> message, CancellationToken cancellationToken = default);

    /// <summary>Schedules a message to be sent to the queue or subscription at the specified time.</summary>
    /// <param name="message">The <see cref="BusMessage{TPayload}"/> to send.</param>
    /// <param name="sendOn">The time at which to schedule the message to be sent to the queue or subscription.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while sending the message.</param>
    /// <returns>The sequence number of the scheduled message.</returns>
    public Task<long> ScheduleMessageAsync(
        BusMessage<TPayload> message,
        DateTimeOffset sendOn,
        CancellationToken cancellationToken = default
    );

    /// <summary>Cancels a previously scheduled message.</summary>
    /// <param name="sequenceNumber">The sequence number of the message to cancel.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while canceling the message.</param>
    /// <exception cref="MessageNotFoundException">
    ///     Thrown when the message with the specified sequence number is not found.
    /// </exception>
    public Task CancelScheduledMessageAsync(long sequenceNumber, CancellationToken cancellationToken = default);
}
