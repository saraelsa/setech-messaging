namespace SETech.Messaging.MessageBus.AzcoBus.MessageProcessing;

/// <summary>
///     The <see cref="MessageLockExpiredException"/> indicates that the settlement operation on a message failed because the
///     lock on the message expired before it could complete.
/// </summary>
public class MessageLockExpiredException : Exception
{
    public MessageLockExpiredException(string lockToken)
        : base()
    {
        LockToken = lockToken;
    }

    /// <summary>The lock token that is expired.</summary>
    public string LockToken { get; set; }

    /// <inheritdoc />
    public override string Message => $"The message lock associated with the lock token {LockToken} is no longer valid.";
}
