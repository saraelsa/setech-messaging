namespace SETech.Messaging.MessageBus.InMemoryImplementation.Bus.FluentBuilder;

/// <summary>
///     The <see cref="InMemoryMessageBusBuilder"/> allows creating an <see cref="InMemoryMessageBus"/> by specifying the
///     queues, topics and subscriptions in the bus.
/// </summary>
public class InMemoryMessageBusBuilder
{
    /// <summary>Creates an <see cref="InMemoryMessageBusBuilder"/>.</summary>
    public InMemoryMessageBusBuilder() { }

    /// <summary>
    ///     Whether this <see cref="InMemoryMessageBusBuilder"/> was used to create an <see cref="InMemoryMessageBus"/>.
    /// </summary>
    public bool Built { get; protected set; } = false;

    /// <summary>The queues and topics to be added to the created <see cref="InMemoryMessageBus"/>.</summary>
    protected IDictionary<string, object> _queues = new Dictionary<string, object>();

    /// <summary>Creates an <see cref="InMemoryMessageBus"/> from the current configuration.</summary>
    public InMemoryMessageBus Build()
    {
        if (Built)
            throw new InvalidOperationException("This builder was already used.");

        Built = true;

        InMemoryMessageBus messageBus = new (_queues);

        return messageBus;
    }

    /// <inheritdoc cref="WithQueue{TPayload}(string, InMemoryQueueOptions)"/>
    public InMemoryMessageBusBuilder WithQueue<TPayload>(string queueName) =>
        WithQueue<TPayload>(queueName, new InMemoryQueueOptions());

    /// <summary>Adds a queue to the message bus to be created.</summary>
    /// <param name="queueName">The name of the queue.</param>
    /// <param name="options">The options of the queue.</param>
    public InMemoryMessageBusBuilder WithQueue<TPayload>(string queueName, InMemoryQueueOptions options)
    {
        _queues[queueName] = new InMemoryQueue<TPayload>(options);

        return this;
    }

    /// <inheritdoc cref="WithTopic{TPayload}(string, Action{InMemoryTopicBuilder{TPayload}}, InMemoryQueueOptions)"/>
    public InMemoryMessageBusBuilder WithTopic<TPayload>
    (
        string topicName,
        Action<InMemoryTopicBuilder<TPayload>> topicBuilderAction
    ) => WithTopic(topicName, topicBuilderAction, new InMemoryQueueOptions());

    /// <summary>Adds a topic to the message bus to be created.</summary>
    /// <param name="topicName">The name of the topic.</param>
    /// <param name="topicBuilderAction">
    ///     The method that sets up an <see cref="InMemoryTopicBuilder{TPayload}"/> to build the topic.
    /// </param>
    /// <param name="options">The options of the topic.</param>
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
