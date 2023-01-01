namespace SETech.Messaging.MessageBus.InMemoryImplementation.Bus;

/// <summary>Stores settlement actions that can be taken on a received message.</summary>
public class ReceivedMessageActions
{
    /// <summary>Completes a message.</summary>
    public required Action Complete { get; init; }

    /// <summary>Renews the lock on a message.</summary>
    public required Action RenewLock { get; init; }

    /// <summary>Abandons a message by returning it to the queue.</summary>
    public required Action Abandon { get; init; }

    /// <summary>Defers a message by keeping it in the list of messages but not maintaining it in the queue.</summary>
    public required Action Defer { get; init; }

    /// <summary>Sends a message to the dead-letter queue.</summary>
    public required Action<string?, string?> DeadLetter { get; init; }
}
