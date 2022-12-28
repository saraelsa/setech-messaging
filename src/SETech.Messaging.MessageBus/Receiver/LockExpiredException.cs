namespace SETech.Messaging.MessageBus.Receiver;

/// <summary>
///     The <see cref="LockExpiredException"/> indicates that the settlement operation on a message failed because the lock
///     on the message expired before it could complete.
/// </summary>
public class LockExpiredException : Exception
{
    public LockExpiredException()
        : base("The lock on the message expired.")
    {}
}
