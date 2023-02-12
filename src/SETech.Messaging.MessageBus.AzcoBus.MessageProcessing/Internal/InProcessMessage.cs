using SETech.Messaging.MessageBus.AzcoBus.MessageEntities;

namespace SETech.Messaging.MessageBus.AzcoBus.MessageProcessing.Internal;

/// <summary>Represents a message in process in a <see cref="MessageQueue"/>.</summary>
public class InProcessMessage
{
    /// <summary>The sequence number of this message.</summary>
    public long SequenceNumber { get; set; }

    /// <summary>The ID used to correlate messages in the same session. This can be any free-form string.</summary>
    /// <remarks>This value is ignored if sessions are not active on the message processing entity.</remarks>
    public string? SessionId { get; set; }

    /// <summary>The time at which this message will be enqueued and available to be received.</summary>
    public DateTimeOffset? ScheduledEnqueueTimeUtc { get; set; }

    /// <summary>The sequence number of this message when it was originally enqueued.</summary>
    public long EnqueuedSequenceNumber { get; set; }

    /// <summary>The time at which this message was accepted into the message processing entity.</summary>
    public DateTimeOffset EnqueuedTimeUtc { get; set; }

    /// <summary>
    ///     The maximum duration for which the message will remain in the queue, after which the message will be dead-lettered.
    /// </summary>
    /// <remarks>The message will not be dead-lettered if it is already in a dead-letter queue.</remarks>
    public TimeSpan TimeToLive { get; set; }

    /// <summary>The time at which this message expires and may not be received.</summary>
    /// <remarks>Calculated as <see cref="EnqueuedTimeUtc"/> + <see cref="TimeToLive"/>.</remarks>
    public DateTimeOffset ExpiresAtUtc => EnqueuedTimeUtc + TimeToLive;

    /// <summary>The number of times delivery of this message was attempted.</summary>
    public int DeliveryCount { get; set; }

    /// <summary>The ID that uniquely identifies this message. This can be any free-form string.</summary>
    public string? MessageId { get; set; }

    /// <summary>The ID used to correlate replies to the requesting message's <see cref="MessageId"/>.</summary>
    public string? CorrelationId { get; set; }

    /// <summary>The address of the queue into which replies to this message are expected.</summary>
    public string? ReplyTo { get; set; }

    /// <summary>For replying to this message, this value should be set on the reply's <see cref="SessionId"/>.</summary>
    public string? ReplyToSessionId { get; set; }

    /// <summary>The application-defined subject of this message.</summary>
    public string? Subject { get; set; }

    /// <summary>The dictionary of application-defined properties on this message.</summary>
    public IDictionary<string, object> ApplicationProperties { get; set; } = new Dictionary<string, object>();

    /// <summary>The RFC2045 section 5 content type of <see cref="Body"/>.</summary>
    public string? ContentType { get; set; }

    /// <summary>The body of the message. This is an opaque property that is not inspected or modified by the bus.</summary>
    public BinaryData? Body { get; set; }

    /// <summary>The handle of the lock that is currently active on this message.</summary>
    public string? LockToken { get; set; }

    /// <summary>The time at which the lock on this message expires.</summary>
    public DateTimeOffset? LockedUntilUtc { get; set; }

    /// <summary>
    ///     Creates an <see cref="InProcessMessage"/> from a <see cref="StoredMessage"/> with extra properties specified.
    /// </summary>
    public static InProcessMessage FromStoredMessage(
        StoredMessage message,
        string? lockToken,
        DateTimeOffset? lockedUntilUtc) =>
        new InProcessMessage()
        {
            SequenceNumber = message.SequenceNumber,
            SessionId = message.SessionId,
            ScheduledEnqueueTimeUtc = message.ScheduledEnqueueTimeUtc,
            EnqueuedSequenceNumber = message.EnqueuedSequenceNumber,
            EnqueuedTimeUtc = message.EnqueuedTimeUtc,
            DeliveryCount = message.DeliveryCount,
            TimeToLive = message.TimeToLive,
            MessageId = message.MessageId,
            CorrelationId = message.CorrelationId,
            ReplyTo = message.ReplyTo,
            ReplyToSessionId = message.ReplyToSessionId,
            Subject = message.Subject,
            ApplicationProperties = message.ApplicationProperties,
            ContentType = message.ContentType,
            Body = message.Body,
            LockToken = lockToken,
            LockedUntilUtc = lockedUntilUtc
        };

    /// <summary>Converts this in-process message to a <see cref="ReceivedBusMessage"/>.</summary>
    public ReceivedBusMessage ToReceivedMessage() =>
        new ReceivedBusMessage()
        {
            SequenceNumber = SequenceNumber,
            SessionId = SessionId,
            ScheduledEnqueueTimeUtc = ScheduledEnqueueTimeUtc,
            EnqueuedSequenceNumber = EnqueuedSequenceNumber,
            EnqueuedTimeUtc = EnqueuedTimeUtc,
            TimeToLive = TimeToLive,
            LockToken = LockToken,
            MessageId = MessageId,
            CorrelationId = CorrelationId,
            ReplyTo = ReplyTo,
            ReplyToSessionId = ReplyToSessionId,
            Subject = Subject,
            ApplicationProperties = ApplicationProperties,
            ContentType = ContentType,
            Body = Body
        };
    
    /// <summary>Converts this in-process message to a <see cref="StoredMessage"/>.</summary>
    public StoredMessage ToStoredMessage() =>
        new StoredMessage()
        {
            SequenceNumber = SequenceNumber,
            SessionId = SessionId,
            ScheduledEnqueueTimeUtc = ScheduledEnqueueTimeUtc,
            EnqueuedSequenceNumber = EnqueuedSequenceNumber,
            EnqueuedTimeUtc = EnqueuedTimeUtc,
            DeliveryCount = DeliveryCount,
            TimeToLive = TimeToLive,
            MessageId = MessageId,
            CorrelationId = CorrelationId,
            ReplyTo = ReplyTo,
            ReplyToSessionId = ReplyToSessionId,
            Subject = Subject,
            ApplicationProperties = ApplicationProperties,
            ContentType = ContentType,
            Body = Body
        };
}