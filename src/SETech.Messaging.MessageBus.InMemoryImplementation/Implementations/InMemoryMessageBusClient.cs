using SETech.Messaging.MessageBus.Client;
using SETech.Messaging.MessageBus.InMemoryImplementation.Bus;
using SETech.Messaging.MessageBus.Primitives;
using SETech.Messaging.MessageBus.Receiver;
using SETech.Messaging.MessageBus.Sender;

namespace SETech.Messaging.MessageBus.InMemoryImplementation.Implementations;

/// <inheritdoc />
public class InMemoryMessageBusClient : IMessageBusClient
{
    /// <summary>The <see cref="InMemoryMessageBus"/> this client is for.</summary>
    public InMemoryMessageBus Bus { get; protected init; }

    /// <summary>Creates an <see cref="InMemoryMessageBusClient"/> using the specified bus.</summary>
    /// <param name="bus">The <see cref="InMemoryMessageBus"/> to create this client for.</param>
    public InMemoryMessageBusClient(InMemoryMessageBus bus)
    {
        Bus = bus;
    }

    public void Dispose() { }

    public IMessageBusReceiver<TPayload> CreateReceiver<TPayload>(string queueName) =>
        CreateReceiver<TPayload>(queueName, new ReceiverOptions());

    public IMessageBusReceiver<TPayload> CreateReceiver<TPayload>(string queueName, ReceiverOptions options)
    {
        InMemoryQueue<TPayload> queue =
            Bus.Queues[queueName] as InMemoryQueue<TPayload> ?? throw new PayloadTypeMismatchException();

        InMemoryMessageBusReceiver<TPayload> receiver = new (queue, options);

        return receiver;
    }

    public IMessageBusReceiver<TPayload> CreateReceiver<TPayload>(string topicName, string subscriptionName) =>
        CreateReceiver<TPayload>(topicName, subscriptionName, new ReceiverOptions());

    public IMessageBusReceiver<TPayload> CreateReceiver<TPayload>
    (
        string topicName,
        string subscriptionName,
        ReceiverOptions options
    )
    {
        InMemoryQueue<TPayload> topicQueue =
            Bus.Queues[topicName] as InMemoryQueue<TPayload> ?? throw new PayloadTypeMismatchException();
        
        InMemoryTopic<TPayload> topic =
            topicQueue as InMemoryTopic<TPayload>
                ?? throw new InvalidOperationException(string.Format("The queue {0} is not a topic.", topicName));
            
        InMemoryQueue<TPayload> subscription = topic.Subscriptions[subscriptionName];

        InMemoryMessageBusReceiver<TPayload> receiver = new (subscription, options);

        return receiver;
    }

    public IMessageBusReplyReceiver<TPayload> CreateReplyReceiver<TPayload>(string queueName)
        where TPayload : ICorrelatedMessagePayload
    {
        throw new NotImplementedException();
    }

    public IMessageBusSender<TPayload> CreateSender<TPayload>(string queueName)
    {
        throw new NotImplementedException();
    }
}
