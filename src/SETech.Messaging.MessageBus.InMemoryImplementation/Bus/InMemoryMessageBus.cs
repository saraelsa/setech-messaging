namespace SETech.Messaging.MessageBus.InMemoryImplementation.Bus;

public class InMemoryMessageBus
{
    public IReadOnlyDictionary<string, object> Queues { get; protected init; }

    public InMemoryMessageBus(IDictionary<string, object> queues)
    {
        Queues = queues.AsReadOnly();
    }
}
