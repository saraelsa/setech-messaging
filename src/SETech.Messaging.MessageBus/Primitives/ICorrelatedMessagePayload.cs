namespace SETech.Messaging.MessageBus.Primitives;

/// <summary>A message that can be correlated with other messages to implement the request-reply pattern.</summary>
/// <remarks>See <see cref="Receiver.IMessageBusReplyReceiver{TEnvelope}"/> for details on this pattern.</remarks>
public interface ICorrelatedMessagePayload
{
    /// <summary>
    ///     The correlation ID of the message. This is a free-form string unique to this conversation and can be used for
    ///     two-way (duplex) communication with <see cref="Receiver.IMessageBusReplyReceiver{TPayload}"/>.
    /// </summary>
    /// <remarks>
    ///     This should be the same for a reply as it is for a request. If multiple messages have to be passed
    ///     back-and-forth, this can be the same for each of them, provided each message is dependent on the previous one
    ///     and will not be sent until that is received.
    /// </remarks>
    public string CorrelationId { get; }
}
