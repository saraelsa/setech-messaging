using SETech.Messaging.MessageBus.Primitives;

namespace SETech.Messaging.MessageBus.InMemoryImplementation.Bus;

/// <summary>The <see cref="InMemoryQueue"/> is a message queue that lives in-memory.</summary>
public class InMemoryQueue<TPayload>
{
    /// <summary>The signature that receiver functions must implement.</summary>
    public delegate void ReceiverFunctionDelegate(ReceivedBusMessage<TPayload> message, ReceivedMessageActions actions);

    /// <summary>The duration for which received messages are locked.</summary>
    public TimeSpan LockDuration { get; protected init; }

    /// <summary>
    ///     The maximum number of times it is attempted to deliver a message before it is sent to the dead-letter queue.
    /// </summary>
    public int MaximumDeliveryAttempts { get; protected init; }

    /// <summary>The maximum time for which a message may remain in the queue.</summary>
    public static TimeSpan MaximumTimeToLive = TimeSpan.FromDays(7);

    /// <summary>
    ///     The next sequence number to use.
    /// </summary>
    protected long NextSequenceNumber { get; set; } = 0;

    /// <summary>The messages that were published to this queue, keyed by their sequence numbers.</summary>
    /// <remarks>Messages are stored until they have been settled as complete.</remarks>
    protected IDictionary<long, StoredMessage<TPayload>> Messages { get; } =
        new SortedDictionary<long, StoredMessage<TPayload>>();

    /// <summary>The sequence numbers of messages that may be received.</summary>
    /// <remarks>The full messages are stored in <see cref="Messages"/>.</remarks>
    protected Queue<long> MessagesSequenceNumberQueue { get; } = new ();

    /// <summary>The sequence numbers of messages in queue order.</summary>
    /// <remarks>The full messages are stored in <see cref="Messages"/>.</remarks>
    protected Queue<long> AllMessagesSequenceNumberQueue { get; set; } = new ();

    /// <summary>The sequence numbers of messages that are currently scheduled.</summary>
    /// <remarks>
    ///     Even though the messages' <see cref="StoredMessage{TPayload}.ScheduledFor"/> property could be used to identify
    ///     scheduled messages, a separate list is stored for performance reasons. The full messages are stored in
    ///     <see cref="Messages"/>.
    /// </remarks>
    protected ICollection<long> ScheduledMessageIds { get; } = new List<long>();

    /// <summary>The pending receiver functions to receive messages.</summary>
    protected Queue<ReceiverFunctionDelegate> ReceiverFunctions { get; } = new();

    /// <summary>Whether this is a dead-letter queue.</summary>
    public bool IsDeadLetterQueue { get; protected init; }

    /// <summary>The dead-letter queue associated with this queue, or null if this itself is a dead-letter queue.</summary>
    public InMemoryQueue<TPayload>? DeadLetterQueue { get; protected init; }

    /// <summary>Creates an <see cref="InMemoryQueue"/> with the specified options.</summary>
    /// <param name="options">The options to create this queue with.</param>
    public InMemoryQueue(InMemoryQueueOptions options)
        : this(options, isDeadLetterQueue: false) { }

    /// <inheritdoc cref="InMemoryQueue(InMemoryQueueOptions)"/>
    /// <param name="isDeadLetterQueue">Whether this is a dead-letter queue.</param>
    protected InMemoryQueue(InMemoryQueueOptions options, bool isDeadLetterQueue)
    {
        IsDeadLetterQueue = isDeadLetterQueue;

        LockDuration = options.LockDuration;
        MaximumDeliveryAttempts = options.MaxDeliveryAttempts;

        if (!IsDeadLetterQueue)
            DeadLetterQueue = new (options, isDeadLetterQueue: true);
    }

    /// <summary>Publishes a message to the queue.</summary>
    /// <param name="message">The message to publish.</param>
    public void Publish(BusMessage<TPayload> message)
    {
        if (IsDeadLetterQueue)
            throw new NotSupportedException("Dead letter queues do not support directly publishing messages to them.");

        PublishInternal(message);
    }

    /// <inheritdoc cref="Publish(BusMessage{TPayload})"/>
    /// <remarks>This internal method does not refuse to publish the message if this is a dead-letter queue.</remarks>
    protected void PublishInternal(BusMessage<TPayload> message)
    {
        StoredMessage<TPayload> storedMessage = new StoredMessage<TPayload>(message, NextSequenceNumber++);

        Messages.Add(storedMessage.SequenceNumber, storedMessage);
        EnqueueSequenceNumber(storedMessage.SequenceNumber);

        TryProcessNext();
    }

    /// <summary>Schedules a message to be sent at a later time.</summary>
    /// <param name="message">The message to schedule.</param>
    /// <param name="sendOn">The time at which to send the message.</param>
    /// <returns>The sequence number of the scheduled message.</returns>
    public long Schedule(BusMessage<TPayload> message, DateTimeOffset sendOn)
    {
        if (IsDeadLetterQueue)
            throw new NotSupportedException("Dead letter queues do not support directly publishing messages to them.");

        StoredMessage<TPayload> storedMessage = new StoredMessage<TPayload>(message, NextSequenceNumber++)
        {
            ScheduledFor = sendOn
        };

        Messages.Add(storedMessage.SequenceNumber, storedMessage);
        AllMessagesSequenceNumberQueue.Enqueue(storedMessage.SequenceNumber);
        ScheduledMessageIds.Add(storedMessage.SequenceNumber);

        return storedMessage.SequenceNumber;
    }

    /// <exception cref="MessageNotFoundException">
    ///     Thrown when no message with the specified sequence number was found.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when the message with the specified sequence number was not scheduled.
    /// </exception>
    public void CancelScheduledMessage(long sequenceNumber)
    {
        if (!Messages.ContainsKey(sequenceNumber))
            throw new MessageNotFoundException(sequenceNumber);

        if (!ScheduledMessageIds.Contains(sequenceNumber))
            throw new InvalidOperationException("Can not cancel a message that is not scheduled in the future.");

        RemoveMessage(sequenceNumber);
        ScheduledMessageIds.Remove(sequenceNumber);
    }

    /// <summary>Receives a message from this queue. A receiver function is called once a message is received.</summary>
    /// <remarks>Messages are received on a first-come-first-serve basis.</remarks>
    /// <param name="receiverFunction">The function to call once a message is received.</param>
    public void Receive(ReceiverFunctionDelegate receiverFunction)
    {
        ReceiverFunctions.Enqueue(receiverFunction);

        TryProcessNext();
    }

    /// <summary>Receives a deferred message from this queue. A receiver function is called with the message.</summary>
    /// <param name="sequenceNumber">The sequence number of the deferred message to receive.</param>
    /// <param name="receiverFunction">The function to call with the received message.</param>
    /// <exception cref="MessageNotFoundException">
    ///     Thrown when no message with the specified sequence number was found.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when the message with the specified sequence number was not deferred.
    /// </exception>
    public void ReceiveDeferred(long sequenceNumber, ReceiverFunctionDelegate receiverFunction)
    {
        if (!Messages.ContainsKey(sequenceNumber))
            throw new MessageNotFoundException(sequenceNumber);

        StoredMessage<TPayload> message = Messages[sequenceNumber];

        if (!message.Deferred)
            throw new InvalidOperationException
            (
                "A message that has not been deferred can not be received as a deferred message."
            );

        ProcessMessageForReceiver(sequenceNumber, receiverFunction);
    }

    /// <summary>Peeks into a message without locking it.</summary>
    /// <param name="fromSequenceNumber">The minimum sequence number of the message to peek.</param>
    public ReceivedBusMessage<TPayload>? Peek(long fromSequenceNumber)
    {
        ReceivedBusMessage<TPayload>? receivedMessage = AllMessagesSequenceNumberQueue
            .Where(sequenceNumber => sequenceNumber >= fromSequenceNumber)
            .Select(sequenceNumber => GenerateReceivedMessage(Messages[sequenceNumber]))
            .FirstOrDefault();

        return receivedMessage;
    }

    /// <summary>Peeks into multiple messages without locking them.</summary>
    /// <param name="fromSequenceNumber">The minimum sequence number of the messages to peek.</param>
    public IList<ReceivedBusMessage<TPayload>> PeekMany(long fromSequenceNumber, int maximumResults)
    {
        IList<ReceivedBusMessage<TPayload>> receivedMessages = AllMessagesSequenceNumberQueue
            .Where(sequenceNumber => sequenceNumber >= fromSequenceNumber)
            .Take(maximumResults)
            .Select(sequenceNumber => GenerateReceivedMessage(Messages[sequenceNumber]))
            .ToList();

        return receivedMessages;
    }

    /// <summary>
    ///     Creates a <see cref="ReceivedBusMessage{TPayload}"/> from a <see cref="StoredMessage{TPayload}"/>.
    /// </summary>
    /// <param name="storedMessage">
    ///     The <see cref="StoredMessage{TPayload}"/> to create the <see cref="ReceivedBusMessage{TPayload}"/> from.
    /// </param>
    protected ReceivedBusMessage<TPayload> GenerateReceivedMessage(StoredMessage<TPayload> storedMessage) =>
        new ReceivedBusMessage<TPayload>()
        {
            MessageId = storedMessage.MessageId,
            SequenceNumber = storedMessage.SequenceNumber,
            TimeToLive = storedMessage.TimeToLive,
            Payload = storedMessage.Payload,
            Deferred = storedMessage.Deferred,
            ScheduledFor = storedMessage.ScheduledFor,
            Timestamp = storedMessage.Timestamp
        };

    /// <summary>Tries to send a message awaiting receival to the first receiver function waiting in line.</summary>
    protected void TryProcessNext()
    {
        if (!IsDeadLetterQueue)
            DeadLetterExpiredMessages();
        
        ProcessScheduledMessages();

        if (MessagesSequenceNumberQueue.Count > 0 && ReceiverFunctions.Count > 0)
        {
            long sequenceNumber = MessagesSequenceNumberQueue.Dequeue();
            ReceiverFunctionDelegate receiverFunction = ReceiverFunctions.Dequeue();

            ProcessMessageForReceiver(sequenceNumber, receiverFunction);
        }
    }

    /// <summary>Sends due scheduled messages to the queue.</summary>
    protected void ProcessScheduledMessages()
    {
        // Enumerates a copy of the list to allow modifying the original list during enumeration.
        foreach (long sequenceNumber in new List<long>(ScheduledMessageIds))
        {
            StoredMessage<TPayload> message = Messages[sequenceNumber];

            if (DateTimeOffset.Now >= message.ScheduledFor)
            {
                message.ScheduledFor = null;
                ScheduledMessageIds.Remove(sequenceNumber);
                EnqueueSequenceNumber(sequenceNumber);
            }
        }
    }

    /// <summary>Sends the message with the specified sequence number to a specified receiver function.</summary>
    /// <param name="sequenceNumber">The sequence number of the message to send.</param>
    /// <param name="receiverFunction">The receiver function to send the message to.</param>
    protected void ProcessMessageForReceiver(long sequenceNumber, ReceiverFunctionDelegate receiverFunction)
    {
        StoredMessage<TPayload> message = Messages[sequenceNumber];

        message.Locked = true;

        ReceivedBusMessage<TPayload> receivedMessage = GenerateReceivedMessage(message);

        MessageLock messageLock = new (LockDuration);

        messageLock.OnExpire += (_, _) => RetryFailedMessage(message);

        ReceivedMessageActions receivedMessageActions = new ()
        {
            RenewLock = messageLock.Renew,
            Complete = messageLock.CreateHandler(() => CompleteMessage(message)),
            Abandon = messageLock.CreateHandler(() => AbandonMessage(message)),
            Defer = messageLock.CreateHandler(() => DeferMessage(message)),
            DeadLetter = (reason, description) =>
                messageLock.CreateHandler(() => DeadLetterMessage(message, reason, description))()
        };

        receiverFunction(receivedMessage, receivedMessageActions);
    }

    /// <summary>
    ///     Dead-letters the next messages in the queue that are expired until a currently valid message, if any, is found.
    /// </summary>
    protected void DeadLetterExpiredMessages()
    {
        if (IsDeadLetterQueue)
            throw new NotSupportedException("Messages in the dead letter queue do not expire.");
        
        if (MessagesSequenceNumberQueue.Count == 0)
            return;

        StoredMessage<TPayload> nextMessage = Messages[MessagesSequenceNumberQueue.Peek()];

        TimeSpan timeToLive;

        if (nextMessage.TimeToLive == null || nextMessage.TimeToLive.Value > MaximumTimeToLive)
            timeToLive = MaximumTimeToLive;
        else
            timeToLive = nextMessage.TimeToLive.Value;

        if (DateTimeOffset.Now > nextMessage.Timestamp + timeToLive)
        {
            MessagesSequenceNumberQueue.Dequeue();
            RemoveMessage(nextMessage.SequenceNumber);

            BusMessage<TPayload> deadLetteredMessage = new ()
            {
                MessageId = nextMessage.MessageId,
                TimeToLive = null,
                Payload = nextMessage.Payload,
                DeadLetterReason = "TTLExpiredException",
                DeadLetterDescription = null
            };

            DeadLetterQueue!.PublishInternal(deadLetteredMessage);

            DeadLetterExpiredMessages();
        }
    }

    /// <summary>
    ///     Retries delivery of a failed message, and moves it into the dead-letter queue if the maximum delivery attempts
    ///     have been exceeded.
    /// </summary>
    /// <param name="message">The message to deliver.</param>
    protected void RetryFailedMessage(StoredMessage<TPayload> message)
    {
        message.DeliveryAttempts++;

        if (!IsDeadLetterQueue && message.DeliveryAttempts > MaximumDeliveryAttempts)
        {
            DeadLetterMessage(message, reason: "MaxDeliveryCountExceeded", description: null);
            return;
        }

        message.Locked = false;
        EnqueueSequenceNumber(message.SequenceNumber);
    }

    /// <summary>Completes a message by removing it from the queue and the list of messages.</summary>
    /// <param name="message">The message to complete.</param>
    protected void CompleteMessage(StoredMessage<TPayload> message)
    {
        RemoveMessage(message.SequenceNumber);
    }

    /// <summary>Abandons a message by returning it to the queue.</summary>
    /// <param name="message">The message to abandon.</param>
    protected void AbandonMessage(StoredMessage<TPayload> message)
    {
        message.Locked = false;
        EnqueueSequenceNumber(message.SequenceNumber);

        TryProcessNext();
    }

    /// <summary>Defers a message by keeping it in the list of messages but not maintaining it in the queue.</summary>
    /// <param name="message">The message to defer.</param>
    protected void DeferMessage(StoredMessage<TPayload> message)
    {
        message.Locked = false;
        message.Deferred = true;
    }

    /// <summary>Sends a message to the dead-letter queue.</summary>
    /// <param name="message">The message to dead-letter.</param>
    protected void DeadLetterMessage(StoredMessage<TPayload> message, string? reason, string? description)
    {
        if (IsDeadLetterQueue)
            throw new NotSupportedException("Messages in the dead letter queue can not be further dead lettered.");

        RemoveMessage(message.SequenceNumber);

        BusMessage<TPayload> deadLetteredMessage = new ()
        {
            MessageId = message.MessageId,
            TimeToLive = null,
            Payload = message.Payload,
            DeadLetterReason = message.DeadLetterReason,
            DeadLetterDescription = message.DeadLetterDescription
        };

        DeadLetterQueue!.PublishInternal(deadLetteredMessage);
    }

    protected void EnqueueSequenceNumber(long sequenceNumber)
    {
        AllMessagesSequenceNumberQueue = new (AllMessagesSequenceNumberQueue
            .Where(sequenceNumberInQueue => sequenceNumberInQueue != sequenceNumber)
            .Append(sequenceNumber)
            .ToArray());
        MessagesSequenceNumberQueue.Enqueue(sequenceNumber);
    }

    protected void RemoveMessage(long sequenceNumber)
    {
        Messages.Remove(sequenceNumber);

        AllMessagesSequenceNumberQueue = new (AllMessagesSequenceNumberQueue
            .Where(sequenceNumberInQueue => sequenceNumberInQueue != sequenceNumber)
            .ToArray());
    }
}
