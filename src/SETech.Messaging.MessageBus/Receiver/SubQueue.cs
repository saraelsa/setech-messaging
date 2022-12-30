namespace SETech.Messaging.MessageBus.Receiver;

/// <summary>The subqueue of the queue to receive from.</summary>
public enum SubQueue
{
    /// <summary>Receive from the main queue.</summary>
    None,

    /// <summary>Receive from the dead-letter queue.</summary>
    DeadLetter
}
