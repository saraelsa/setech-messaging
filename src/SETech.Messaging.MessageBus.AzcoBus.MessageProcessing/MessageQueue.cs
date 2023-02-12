using System.Collections.Concurrent;
using SETech.Messaging.MessageBus.AzcoBus.MessageEntities;
using SETech.Messaging.MessageBus.AzcoBus.MessageProcessing.Internal;
using SETech.Messaging.MessageBus.AzcoBus.MessageProcessing.Ports;

namespace SETech.Messaging.MessageBus.AzcoBus.MessageProcessing;

public sealed class MessageQueue
{
    public TimeSpan MessageLockDuration { get; } = TimeSpan.FromSeconds(30);

    private IBackingMessageQueue _backingMessageQueue;

    private ConcurrentQueue<ReceiveRequest> _pendingReceiveRequests = new ();

    private bool _isMessageReceiving = false;
    private InProcessMessage? _inProcessMessage;
    private object _messageReceivingLock = new ();

    private string? _currentLockToken;
    private Timer? _lockExpirationTimer;
    private object _messageSettlingLock = new ();

    public MessageQueue(IBackingMessageQueue backingMessageQueue)
    {
        _backingMessageQueue = backingMessageQueue;
    }

    public async Task PublishMessageAsync(BusMessage message, CancellationToken cancellationToken = default)
    {
        long sequenceNumber = await _backingMessageQueue.AllocateSequenceNumber(cancellationToken);
        DateTimeOffset enqueueTime = DateTimeOffset.UtcNow;

        StoredMessage storedMessage = StoredMessage.FromMessage(
            message,
            sequenceNumber,
            scheduledEnqueueTimeUtc: null,
            enqueuedSequenceNumber: sequenceNumber,
            enqueuedTimeUtc: enqueueTime,
            deliveryCount: 0);

        await _backingMessageQueue.EnqueueMessage(storedMessage, cancellationToken);
    }

    public async Task ScheduleMessageAsync(
        BusMessage message,
        DateTimeOffset scheduledEnqueueTimeUtc,
        CancellationToken cancellationToken = default)
    {
        long sequenceNumber = await _backingMessageQueue.AllocateSequenceNumber(cancellationToken);
        DateTimeOffset enqueueTime = DateTimeOffset.UtcNow;

        StoredMessage storedMessage = StoredMessage.FromMessage(
            message,
            sequenceNumber,
            scheduledEnqueueTimeUtc,
            enqueuedSequenceNumber: sequenceNumber,
            enqueuedTimeUtc: enqueueTime,
            deliveryCount: 0);
        
        await _backingMessageQueue.EnqueueScheduledMessage(storedMessage, cancellationToken);
    }

    public Task<ReceivedBusMessage> ReceiveMessageAsync(CancellationToken cancellationToken = default)
    {
        TaskCompletionSource<ReceivedBusMessage> taskCompletionSource = new (cancellationToken);

        ReceiveRequest receiveRequest = new ()
        {
            TaskCompletionSource = taskCompletionSource,
            CancellationToken = cancellationToken
        };

        _pendingReceiveRequests.Enqueue(receiveRequest);

        ProcessReceiveRequest();

        return taskCompletionSource.Task;
    }

    public async Task<ReceivedBusMessage> ReceiveDeferredMessageAsync(
        long sequenceNumber,
        CancellationToken cancellationToken = default)
    {
        StoredMessage storedMessage
            = await _backingMessageQueue.DequeueDeferredMessage(sequenceNumber, cancellationToken)
                ?? throw new MessageNotFoundException(sequenceNumber);
            
        InProcessMessage inProcessMessage = InProcessMessage.FromStoredMessage(
            storedMessage,
            lockToken: null,
            lockedUntilUtc: null);

        return inProcessMessage.ToReceivedMessage();
    }

    public void RenewMessageLock(string lockToken)
    {
        if (_currentLockToken != lockToken)
            throw new MessageLockExpiredException(lockToken);
        
        SetMessageLockTimer(lockToken);
    }

    public async Task CompleteMessageAsync(string lockToken, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        VoidMessageLock(lockToken);
        await _backingMessageQueue.DequeueMessage();

        _isMessageReceiving = false;

        ProcessReceiveRequest();
    }

    public async Task DeferMessageAsync(string lockToken, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        VoidMessageLock(lockToken);

        // We perform these operations sequentially so that if the application crashes the message is not lost.
        await _backingMessageQueue.EnqueueDeferredMessage(_inProcessMessage!.ToStoredMessage());
        await _backingMessageQueue.DequeueMessage();

        _isMessageReceiving = false;

        ProcessReceiveRequest();
    }

    public async Task AbandonMessageAsync(string lockToken, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        VoidMessageLock(lockToken);
        _inProcessMessage!.DeliveryCount++;
        await _backingMessageQueue.UpdateMessage(_inProcessMessage.ToStoredMessage());

        _isMessageReceiving = false;

        ProcessReceiveRequest();
    }

    private async void ProcessReceiveRequest()
    {
        // Ensure there's only one attempt to receive a message at any given time.
        lock (_messageReceivingLock)
        {
            if (_isMessageReceiving)
                return;

            _isMessageReceiving = true;
        }

        ReceiveRequest? receiveRequest;

        // If there are no requests to receive a message, exit the method. Peek is used instead of dequeue because the method
        // may exit without processing the receive request.
        if (!_pendingReceiveRequests.TryPeek(out receiveRequest))
        {
            _isMessageReceiving = false;
            return;
        }

        if (receiveRequest.CancellationToken.IsCancellationRequested)
        {
            _pendingReceiveRequests.TryDequeue(out _);
            _isMessageReceiving = false;
            ProcessReceiveRequest();
            return;
        }

        // Ensure any due scheduled messages are added to the queue so they can be received.
        await ProcessScheduledMessages();

        // Peeks the message instead of dequeuing because the application might crash between here and the message's settlement.
        // We will only dequeue it once it has been settled. This preserves the at-least-once guarantee. It also allows
        // abandoning the message.
        StoredMessage? nextStoredMessage = await GetNextUnexpiredMessage();

        if (nextStoredMessage is null)
        {
            _isMessageReceiving = false;
            return;
        }
        
        _pendingReceiveRequests.TryDequeue(out _);

        lock (_messageSettlingLock)
        {
            string lockToken = AcquireMessageLock();
            _inProcessMessage = InProcessMessage.FromStoredMessage(
                nextStoredMessage,
                lockToken,
                lockedUntilUtc: DateTimeOffset.UtcNow + MessageLockDuration);
        }

        receiveRequest.TaskCompletionSource.SetResult(_inProcessMessage.ToReceivedMessage());
    }

    private async Task ProcessScheduledMessages()
    {
        StoredMessage? nextScheduledMessage = await _backingMessageQueue.PeekScheduledMessage();

        while (nextScheduledMessage is not null && DateTimeOffset.UtcNow >= nextScheduledMessage.ScheduledEnqueueTimeUtc)
        {
            await _backingMessageQueue.TransferScheduledMessage();

            nextScheduledMessage = await _backingMessageQueue.PeekScheduledMessage();
        }
    }

    private async Task<StoredMessage?> GetNextUnexpiredMessage()
    {
        StoredMessage? nextMessage = await _backingMessageQueue.DequeueMessage();

        while (nextMessage is not null && DateTimeOffset.UtcNow >= nextMessage.ExpiresAtUtc)
            nextMessage = await _backingMessageQueue.DequeueMessage();
        
        return nextMessage;
    }

    private string AcquireMessageLock()
    {
        string lockToken = GenerateMessageLockToken();
        SetMessageLockTimer(lockToken);

        return lockToken;
    }

    private string GenerateMessageLockToken() => Guid.NewGuid().ToString();

    private void SetMessageLockTimer(string lockToken)
    {
        if (_lockExpirationTimer is not null)
            _lockExpirationTimer.Dispose();
        
        _lockExpirationTimer = new Timer(
            callback: state => HandleMessageLockExpiry((string)state!),
            state: lockToken,
            dueTime: ((int)MessageLockDuration.TotalMilliseconds),
            period: Timeout.Infinite);
    }

    private void VoidMessageLock(string lockToken)
    {
        lock (_messageSettlingLock)
        {
            if (_currentLockToken != lockToken)
                throw new MessageLockExpiredException(lockToken);

            _currentLockToken = null;
        }
    }

    private void HandleMessageLockExpiry(string lockToken)
    {
        bool lockWasValid = false;

        lock (_messageSettlingLock)
        {
            if (_currentLockToken == lockToken)
            {
                _currentLockToken = null;
                _isMessageReceiving = false;

                lockWasValid = true;
            }
        }

        if (lockWasValid)
            ProcessReceiveRequest();
    }
}
