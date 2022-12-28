namespace SETech.Messaging.MessageBus.Receiver;

/// <summary>The mode to receive messages with. This controls how messages are settled.</summary>
public enum ReceiveMode
{
    /// <summary>
    ///     In the <see cref="PeekLock"/> mode, once a message is received, it is locked for a certain period of time,
    ///     during which it may be settled, deferred or moved to the dead-letter queue. The lock may be renewed
    ///     indefinitely. If the lock expires, the message is returned to its queue.
    /// </summary>
    PeekLock,

    /// <summary>
    ///     In the <see cref="ReceiveAndDelete"/> mode, once a message is received, it is automatically settled.
    /// <summary>
    ReceiveAndDelete
}
