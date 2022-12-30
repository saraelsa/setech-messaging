namespace SETech.Messaging.MessageBus.InMemoryImplementation.Bus;

public class InMemoryMessageBus
{
    protected IDictionary<string, InMemoryQueue<object>> _queues = new Dictionary<string, InMemoryQueue<object>>();

    public IReadOnlyDictionary<string, InMemoryQueue<object>> Queues => _queues.AsReadOnly();
}
