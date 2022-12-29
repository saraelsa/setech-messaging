using SETech.Messaging.MessageBus.Primitives;

namespace SETech.Messaging.MessageBus.InMemoryImplementation.Bus;

public class InMemoryTopic<TPayload> : InMemoryQueue<TPayload>
{
    protected IDictionary<string, InMemoryQueue<TPayload>> Subscriptions =
        new Dictionary<string, InMemoryQueue<TPayload>>();
    
    public InMemoryTopic()
    {
        Receive(ReceiverFunction);
    }

    protected void ReceiverFunction(ReceivedBusMessage<TPayload> receivedMessage, ReceivedMessageActions actions)
    {
        foreach (InMemoryQueue<TPayload> subscription in Subscriptions.Values)
            subscription.Publish(receivedMessage);
        
        actions.Complete();
    }
}
