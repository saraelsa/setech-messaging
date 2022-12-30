using SETech.Messaging.MessageBus.Primitives;

namespace SETech.Messaging.MessageBus.Receiver;

/// <summary>The <see cref="IMessageBusReplyReceiver"/> receives replies from a queue.</summary>
/// <remarks>
///     Sometimes, a service needs to send a message and then receive a reply for that message. One way to accomplish this
///     is by using a separate receiving queue to receive replies. The service that receives the message will publish a
///     reply to this queue with a correlation ID that matches the correlation ID of the message being replied to. This
///     reply receiver abstracts away the requirement of having to filter through all received messages on the receiving
///     queue, allowing the consumer needing the reply to provide the correlation ID and providing to them the reply they
///     requested as the result of a task. The <see cref="ReceiveMode"/> is always set to
///     <see cref="ReceiveMode.ReceiveAndDelete"/>.
/// </remarks>
/// <typeparam name="TPayload">The payload type to receive.</typeparam>
public interface IMessageBusReplyReceiver<TPayload> : IDisposable
    where TPayload : ICorrelatedMessagePayload
{
    /// <summary>Receives the next message with the specified <paramref name="correlationId"/>.</summary>
    /// <param name="correlationId">The correlation ID of the reply to receive.</param>
    /// <param name="cancellationToken">
    ///     A <see cref="CancellationToken"/> to observe while receiving the reply.
    /// </param>
    public Task<ReceivedBusMessage<TPayload>> ReceiveReplyAsync
    (
        string correlationId,
        CancellationToken cancellationToken = default
    );
}
