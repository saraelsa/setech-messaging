namespace SETech.Messaging.MessageBus.InMemoryImplementation.Bus;

/// <summary>The options that can be configured while creating an <see cref="InMemoryQueue{TPayload}"/>.</summary>
public class InMemoryQueueOptions
{
    /// <summary>The duration for which received messages are locked.</summary>
    public TimeSpan LockDuration = TimeSpan.FromSeconds(30);

    /// <summary>
    ///     The maximum number of times it is attempted to deliver a message before it is sent to the dead-letter queue.
    /// </summary>
    public int MaxDeliveryAttempts = 10;
}
