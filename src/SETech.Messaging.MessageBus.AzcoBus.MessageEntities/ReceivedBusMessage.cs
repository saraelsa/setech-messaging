namespace SETech.Messaging.MessageBus.AzcoBus.MessageEntities;

public class ReceivedBusMessage
{
    /// <summary>The sequence number of this message.</summary>
    public required long SequenceNumber { get; set; }

    /// <summary>The ID used to correlate messages in the same session. This can be any free-form string.</summary>
    /// <remarks>This value is ignored if sessions are not active on the message processing entity.</remarks>
    public string? SessionId { get; set; }

    /// <summary>The time at which this message will be enqueued and available to be received.</summary>
    public DateTimeOffset? ScheduledEnqueueTimeUtc { get; set; }

    /// <summary>The sequence number of this message when it was originally enqueued.</summary>
    public required long EnqueuedSequenceNumber { get; set; }

    /// <summary>The time at which this message was accepted into the message processing entity.</summary>
    public required DateTimeOffset EnqueuedTimeUtc { get; set; }

    /// <summary>
    ///     The maximum duration for which the message will remain in the queue, after which the message will be dead-lettered.
    /// </summary>
    /// <remarks>The message will not be dead-lettered if it is already in a dead-letter queue.</remarks>
    public required TimeSpan TimeToLive { get; set; }

    /// <summary>The time at which this message expires and may not be received.</summary>
    /// <remarks>Calculated as <see cref="EnqueuedTimeUtc"/> + <see cref="TimeToLive"/>.</remarks>
    public DateTimeOffset ExpiresAtUtc => EnqueuedTimeUtc + TimeToLive;

    /// <summary>The handle to the lock on this message that can be used for settlement and deferment operations.</summary>
    public string? LockToken { get; set; }

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
    public required IDictionary<string, object> ApplicationProperties { get; set; } = new Dictionary<string, object>();

    /// <summary>The RFC2045 section 5 content type of <see cref="Body"/>.</summary>
    public string? ContentType { get; set; }

    /// <summary>The body of the message. This is an opaque property that is not inspected or modified by the bus.</summary>
    public BinaryData? Body { get; set; }
}