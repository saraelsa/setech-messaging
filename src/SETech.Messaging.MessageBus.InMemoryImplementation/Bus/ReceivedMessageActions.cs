namespace SETech.Messaging.MessageBus.InMemoryImplementation.Bus;

public class ReceivedMessageActions
{
    public required Action Complete { get; init; }

    public required Action RenewLock { get; init; }

    public required Action Abandon { get; init; }

    public required Action Defer { get; init; }

    public required Action DeadLetter { get; init; }
}
