using SETech.Messaging.MessageBus.Primitives;

namespace SETech.Messaging.MessageBus.InMemoryImplementation.Bus;

public class InMemoryTopic<TPayload> : InMemoryQueue<TPayload>
{
    protected IDictionary<string, InMemoryQueue<TPayload>> _subscriptions =
        new Dictionary<string, InMemoryQueue<TPayload>>();
    
    public IReadOnlyDictionary<string, InMemoryQueue<TPayload>> Subscriptions => _subscriptions.AsReadOnly();

    public InMemoryTopic(IDictionary<string, InMemoryQueue<TPayload>> subscriptions)
        : this(subscriptions, new InMemoryQueueOptions()) { }
    
    public InMemoryTopic(IDictionary<string, InMemoryQueue<TPayload>> subscriptions, InMemoryQueueOptions options)
        : base(options)
    {
        _subscriptions = subscriptions.AsReadOnly();

        Receive(ReceiverFunction);
    }

    protected void ReceiverFunction(ReceivedBusMessage<TPayload> receivedMessage, ReceivedMessageActions actions)
    {
        foreach (InMemoryQueue<TPayload> subscription in Subscriptions.Values)
            subscription.Publish(receivedMessage);
        
        actions.Complete();
    }
}
