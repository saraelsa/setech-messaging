namespace SETech.Messaging.MessageBus.Dispatcher;

/// <summary>The options to use when dispatching a message.</summary>
public class MessageDispatchOptions
{
    /// <summary>The duration after which the message expires.</summary>
    public TimeSpan? TimeToLive { get; init; }
}
