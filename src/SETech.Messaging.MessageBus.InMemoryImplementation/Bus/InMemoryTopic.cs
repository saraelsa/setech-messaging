using SETech.Messaging.MessageBus.Primitives;

namespace SETech.Messaging.MessageBus.InMemoryImplementation.Bus;

public class InMemoryTopic<TPayload> : InMemoryQueue<TPayload>
{
    protected IDictionary<string, InMemoryQueue<TPayload>> _subscriptions =
        new Dictionary<string, InMemoryQueue<TPayload>>();
    
    public IReadOnlyDictionary<string, InMemoryQueue<TPayload>> Subscriptions => _subscriptions.AsReadOnly();
    
    public InMemoryTopic()
    {
        Receive(ReceiverFunction);
    }

    public InMemoryQueue<TPayload> CreateSubscription(string subscriptionName)
    {
        if (!_subscriptions.ContainsKey(subscriptionName))
            _subscriptions[subscriptionName] = new InMemoryQueue<TPayload>();
        
        return _subscriptions[subscriptionName];
    }

    public void DestroySubscription(string subscriptionName)
    {
        _subscriptions.Remove(subscriptionName);
    }

    protected void ReceiverFunction(ReceivedBusMessage<TPayload> receivedMessage, ReceivedMessageActions actions)
    {
        foreach (InMemoryQueue<TPayload> subscription in Subscriptions.Values)
            subscription.Publish(receivedMessage);
        
        actions.Complete();
    }
}
