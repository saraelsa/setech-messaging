namespace SETech.Messaging.MessageBus.InMemoryImplementation.Bus.FluentBuilder;

public class InMemoryTopicBuilder<TPayload>
{
    public InMemoryTopicBuilder() { }

    protected IDictionary<string, InMemoryQueue<TPayload>> _subscriptions =
        new Dictionary<string, InMemoryQueue<TPayload>>();

    public InMemoryTopic<TPayload> Build()
    {
        InMemoryTopic<TPayload> topic = new (_subscriptions);

        return topic;
    }

    /// <inheritdoc cref="WithQueue(string, InMemoryQueueOptions)"/>
    public InMemoryTopicBuilder<TPayload> WithSubscription(string queueName) =>
        WithSubscription(queueName, new InMemoryQueueOptions());

    public InMemoryTopicBuilder<TPayload> WithSubscription(string queueName, InMemoryQueueOptions options)
    {
        _subscriptions.Add(queueName, new InMemoryQueue<TPayload>(options));

        return this;
    }
}
