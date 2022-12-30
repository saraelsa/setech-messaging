namespace SETech.Messaging.MessageBus.InMemoryImplementation.Bus;

/// <summary>The options that can be configured while creating an <see cref="InMemoryQueue{TPayload}"/>.</summary>
public class InMemoryQueueOptions
{
    /// <summary>The duration to lock messages with.</summary>   
    public TimeSpan LockDuration = TimeSpan.FromSeconds(30);

    /// <summary>The maximum number of times to try to deliver a message, in case deliveries are failing.</summary>   
    public int MaxDeliveryAttempts = 10;
}
