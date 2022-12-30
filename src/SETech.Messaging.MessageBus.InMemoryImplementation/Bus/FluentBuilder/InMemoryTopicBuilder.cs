namespace SETech.Messaging.MessageBus.InMemoryImplementation.Bus.FluentBuilder;

/// <summary>
///     The <see cref="InMemoryMessageBusBuilder"/> allows creating an <see cref="InMemoryTopic{TPayload}"/> by specifying
///     its subscriptions.
/// </summary>
public class InMemoryTopicBuilder<TPayload>
{
    /// <summary>Creates an <see cref="InMemoryTopicBuilder"/>.</summary>
    public InMemoryTopicBuilder() { }

    /// <summary>The subscriptions to be added to the created <see cref="InMemoryTopic{TPayload}"/>.</summary>
    protected IDictionary<string, InMemoryQueue<TPayload>> _subscriptions =
        new Dictionary<string, InMemoryQueue<TPayload>>();

    /// <summary>Creates an <see cref="InMemoryTopic{TPayload}"/> from the current configuration.</summary>
    public InMemoryTopic<TPayload> Build()
    {
        InMemoryTopic<TPayload> topic = new (_subscriptions);

        return topic;
    }

    /// <inheritdoc cref="WithSubscription{TPayload}(string, InMemoryQueueOptions)"/>
    public InMemoryTopicBuilder<TPayload> WithSubscription(string subscriptionName) =>
        WithSubscription(subscriptionName, new InMemoryQueueOptions());

    /// <summary>Adds a subscription to the topic to be created.</summary>
    /// <param name="subscriptionName">The name of the subscription.</param>
    /// <param name="options">The options of the subscription.</param>
    public InMemoryTopicBuilder<TPayload> WithSubscription(string subscriptionName, InMemoryQueueOptions options)
    {
        _subscriptions.Add(subscriptionName, new InMemoryQueue<TPayload>(options));

        return this;
    }
}
