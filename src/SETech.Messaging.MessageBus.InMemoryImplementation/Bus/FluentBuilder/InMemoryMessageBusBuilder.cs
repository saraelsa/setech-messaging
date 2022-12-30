namespace SETech.Messaging.MessageBus.InMemoryImplementation.Bus.FluentFactory;

public class InMemoryMessageBusBuilder
{
    public InMemoryMessageBusBuilder() { }

    protected IDictionary<string, object> _queues = new Dictionary<string, object>();
    
    public InMemoryMessageBus Build()
    {
        InMemoryMessageBus messageBus = new (_queues);

        return messageBus;
    }

    /// <inheritdoc cref="WithQueue(string, InMemoryQueueOptions)"/>
    public InMemoryMessageBusBuilder WithQueue<TPayload>(string queueName) =>
        WithQueue<TPayload>(queueName, new InMemoryQueueOptions());

    public InMemoryMessageBusBuilder WithQueue<TPayload>(string queueName, InMemoryQueueOptions options)
    {
        _queues[queueName] = new InMemoryQueue<TPayload>(options);

        return this;
    }

    public InMemoryMessageBusBuilder WithTopic<TPayload>
    (
        string topicName,
        Action<InMemoryTopicBuilder<TPayload>> topicBuilderAction
    ) => WithTopic(topicName, topicBuilderAction, new InMemoryQueueOptions());

    public InMemoryMessageBusBuilder WithTopic<TPayload>
    (
        string topicName,
        Action<InMemoryTopicBuilder<TPayload>> topicBuilderAction,
        InMemoryQueueOptions options
    )
    {
        InMemoryTopicBuilder<TPayload> topicBuilder = new ();
        topicBuilderAction(topicBuilder);

        InMemoryTopic<TPayload> topic = topicBuilder.Build();

        _queues[topicName] = topic;

        return this;
    }
}
