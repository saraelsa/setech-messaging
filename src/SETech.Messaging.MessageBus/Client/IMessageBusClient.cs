using SETech.Messaging.MessageBus.Primitives;
using SETech.Messaging.MessageBus.Receiver;
using SETech.Messaging.MessageBus.Sender;

namespace SETech.Messaging.MessageBus.Client;

/// <summary>
///     The <see cref="IMessageBusClient"/> represents a connection to the message bus and can be used to create other types
///     used for interacting with the message bus.
/// </summary>
public interface IMessageBusClient : IDisposable
{
    /// <summary>Creates a <see cref="IMessageBusSender{TPayload}"/>.</summary>
    /// <typeparam name="TPayload">The payload type to send.</typeparam>
    /// <param name="queueName">The name of the queue to send messages to.</param>
    /// <returns>The created <see cref="IMessageBusSender"/>.</returns>
    public IMessageBusSender<TPayload> CreateSender<TPayload>(string queueName);

    /// <summary>Creates a <see cref="IMessageBusReceiver{TPayload}"/> for a queue.</summary>
    /// <typeparam name="TPayload">The payload type to receive.</typeparam>
    /// <param name="queueName">The name of the queue to receive messages from.</param>
    /// <returns>The created <see cref="IMessageBusReceiver{TPayload}"/>.</returns>
    public IMessageBusReceiver<TPayload> CreateReceiver<TPayload>(string queueName);

    /// <summary>Creates a <see cref="IMessageBusReceiver{TPayload}"/> for a subscription to a topic.</summary>
    /// <typeparam name="TPayload">The payload type to receive.</typeparam>
    /// <param name="topicName">The name of the topic to receive messages from.</param>
    /// <param name="subscriptionName">The name of the subscription to receive messages from.</param>
    /// <returns>The created <see cref="IMessageBusReceiver{TPayload}"/>.</returns>
    public IMessageBusReceiver<TPayload> CreateReceiver<TPayload>(string topicName, string subscriptionName);

    /// <summary>Creates a <see cref="IMessageBusReplyReceiver{TPayload}"/> for a queue.</summary>
    /// <typeparam name="TPayload">The payload type to receive.</typeparam>
    /// <param name="queueName">The name of the queue to receive messages from.</param>
    /// <returns>The created <see cref="IMessageBusReplyReceiver{TPayload}"/>.</returns>
    public IMessageBusReplyReceiver<TPayload> CreateReplyReceiver<TPayload>(string queueName)
        where TPayload : ICorrelatedMessagePayload;
}
