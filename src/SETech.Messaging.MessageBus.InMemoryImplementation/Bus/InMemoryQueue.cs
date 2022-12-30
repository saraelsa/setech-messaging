using SETech.Messaging.MessageBus.Primitives;

namespace SETech.Messaging.MessageBus.InMemoryImplementation.Bus;

public class InMemoryQueue<TPayload>
{
    public delegate void ReceiverFunctionDelegate(ReceivedBusMessage<TPayload> message, ReceivedMessageActions actions);

    public TimeSpan LockDuration { get; protected init; }
    
    public int MaximumDeliveryAttempts { get; protected init; }

    protected long LastSequenceNumber { get; set; } = 0;

    protected long LastProcessedSequenceNumber { get; set; } = 0;

    protected IDictionary<long, StoredMessage<TPayload>> Messages { get; } =
        new SortedDictionary<long, StoredMessage<TPayload>>();

    protected Queue<long> MessagesSequenceNumberQueue { get; } = new ();

    protected Queue<ReceiverFunctionDelegate> ReceiverFunctions { get; } = new();

    public bool IsDeadLetterQueue { get; protected init; }

    public InMemoryQueue<TPayload>? DeadLetterQueue { get; protected init; }

    public InMemoryQueue(InMemoryQueueOptions options)
        : this(options, isDeadLetterQueue: false) { }

    protected InMemoryQueue(InMemoryQueueOptions options, bool isDeadLetterQueue)
    {
        IsDeadLetterQueue = isDeadLetterQueue;

        LockDuration = options.LockDuration;
        MaximumDeliveryAttempts = options.MaxDeliveryAttempts;

        if (!IsDeadLetterQueue)
            DeadLetterQueue = new (options, isDeadLetterQueue: true);
    }

    public void Publish(BusMessage<TPayload> message)
    {
        if (IsDeadLetterQueue)
            throw new NotSupportedException("Dead letter queues do not support directly publishing messages to them.");

        PublishInternal(message);
    }

    protected void PublishInternal(BusMessage<TPayload> message)
    {
        StoredMessage<TPayload> storedMessage = new StoredMessage<TPayload>(message, LastSequenceNumber++);

        Messages.Add(storedMessage.SequenceNumber, storedMessage);
        MessagesSequenceNumberQueue.Enqueue(storedMessage.SequenceNumber);

        TryProcessNext();
    }

    public void Receive(ReceiverFunctionDelegate receiverFunction)
    {
        ReceiverFunctions.Enqueue(receiverFunction);

        TryProcessNext();
    }

    public ReceivedBusMessage<TPayload>? Peek(long fromSequenceNumber)
    {
        long sequenceNumber = Messages.Keys.FirstOrDefault(sequenceNumber => sequenceNumber > fromSequenceNumber);

        if (sequenceNumber == default)
            return null;
        
        StoredMessage<TPayload> message = Messages[sequenceNumber];

        ReceivedBusMessage<TPayload> receivedMessage = new ()
        {
            MessageId = message.MessageId,
            SequenceNumber = message.SequenceNumber,
            TimeToLive = message.TimeToLive,
            Payload = message.Payload
        };

        return receivedMessage;
    }

    public IList<ReceivedBusMessage<TPayload>> PeekMany(long fromSequenceNumber, int maximumResults)
    {
        IList<ReceivedBusMessage<TPayload>> receivedMessages = Messages
            .Where(messagePair => messagePair.Key >= fromSequenceNumber)
            .Take(maximumResults)
            .Select(messagePair => new ReceivedBusMessage<TPayload>()
            {
                MessageId = messagePair.Value.MessageId,
                SequenceNumber = messagePair.Value.SequenceNumber,
                TimeToLive = messagePair.Value.TimeToLive,
                Payload = messagePair.Value.Payload
            })
            .ToList();

        return receivedMessages;
    }

    protected void TryProcessNext()
    {
        if (!IsDeadLetterQueue)
            DeadLetterExpiredMessages();

        if (MessagesSequenceNumberQueue.Count > 0 && ReceiverFunctions.Count > 0)
        {
            StoredMessage<TPayload> message = Messages[MessagesSequenceNumberQueue.Dequeue()];
            ReceiverFunctionDelegate receiverFunction = ReceiverFunctions.Dequeue();

            Messages.Remove(message.SequenceNumber);

            ReceivedBusMessage<TPayload> receivedMessage = new ()
            {
                MessageId = message.MessageId,
                SequenceNumber = message.SequenceNumber,
                TimeToLive = message.TimeToLive,
                Payload = message.Payload
            };

            MessageLock messageLock = new (LockDuration);

            messageLock.OnExpire += (_, _) => RetryFailedMessage(message);

            ReceivedMessageActions receivedMessageActions = new ()
            {
                RenewLock = messageLock.Renew,
                Complete = messageLock.CreateHandler(() => { }),
                Abandon = () =>  messageLock.CreateHandler(() => AbandonMessage(message)),
                Defer = () => messageLock.CreateHandler(() => DeferMessage(message)),
                DeadLetter = () => messageLock.CreateHandler(() => DeadLetterMessage(message))
            };

            receiverFunction(receivedMessage, receivedMessageActions);
        }
    }

    protected void DeadLetterExpiredMessages()
    {
        if (IsDeadLetterQueue)
            throw new NotSupportedException("Messages in the dead letter queue do not expire.");

        StoredMessage<TPayload> nextMessage = Messages[MessagesSequenceNumberQueue.Peek()];

        if (DateTime.Now - nextMessage.Timestamp > nextMessage.TimeToLive)
        {
            MessagesSequenceNumberQueue.Dequeue();
            Messages.Remove(nextMessage.SequenceNumber);

            DeadLetterQueue!.PublishInternal(nextMessage);

            DeadLetterExpiredMessages();
        }
    }

    protected void RetryFailedMessage(StoredMessage<TPayload> message)
    {
        message.DeliveryAttempts++;

        if (!IsDeadLetterQueue && message.DeliveryAttempts > MaximumDeliveryAttempts)
        {
            DeadLetterMessage(message);
            return;
        }

        Messages[message.SequenceNumber] = message;
        MessagesSequenceNumberQueue.Enqueue(message.SequenceNumber);
    }

    protected void AbandonMessage(StoredMessage<TPayload> message)
    {
        Messages[message.SequenceNumber] = message;
        MessagesSequenceNumberQueue.Enqueue(message.SequenceNumber);
    }

    protected void DeferMessage(StoredMessage<TPayload> message) =>
        Messages.Add(message.SequenceNumber, message);

    protected void DeadLetterMessage(StoredMessage<TPayload> message)
    {
        if (IsDeadLetterQueue)
            throw new NotSupportedException("Messages in the dead letter queue can not be further dead lettered.");

        DeadLetterQueue!.PublishInternal(message);
    }
}
