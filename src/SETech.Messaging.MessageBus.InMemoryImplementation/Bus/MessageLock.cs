using SETech.Messaging.MessageBus.Receiver;

namespace SETech.Messaging.MessageBus.InMemoryImplementation.Bus;

/// <summary>Handles locking a message for a period of time.</summary>
public sealed class MessageLock
{
    /// <summary>The duration for which the message is locked.</summary>
    public TimeSpan LockDuration { get; private init; }

    /// <summary>Whether the lock has expired.</summary>
    public bool Expired { get; private set; }

    /// <summary>Whether the lock has been cleared.</summary>
    public bool Cleared { get; private set; }

    /// <summary>Raised when the lock has expired.</summary>
    public event EventHandler? OnExpire;

    /// <summary>The time at which the lock expires.</summary>
    private DateTimeOffset _expiresOn;

    /// <summary>Creates a <see cref="MessageLock"/>.</summary>
    public MessageLock(TimeSpan lockDuration)
    {
        LockDuration = lockDuration;
        Renew();
        CheckForExpiry();
    }

    /// <summary>
    ///     Wraps an action in a handler that will throw <see cref="LockExpiredException"/> if the message is expired,
    ///     <see cref="InvalidOperationException"/> if the message is cleared, and clears the message otherwise.
    /// </summary>
    public Action CreateHandler(Action protectedAction) =>
        () =>
        {
            if (Expired)
                throw new LockExpiredException();

            if (Cleared)
                throw new InvalidOperationException("The message has already been settled.");

            Cleared = true;

            protectedAction();
        };
    
    /// <summary>Renews the lock for <see cref="LockDuration"/>.</summary>
    public void Renew()
    {
        _expiresOn = DateTimeOffset.Now + LockDuration;
    }

    /// <summary>Checks whether the lock has expired and handles the expiry if so.</summary>
    private void CheckForExpiry()
    {
        if (DateTimeOffset.Now < _expiresOn)
        {
            Task.Delay(LockDuration).ContinueWith(_ => CheckForExpiry());
            return;
        }

        if (!Cleared)
        {
            Cleared = true;

            if (OnExpire is not null)
                OnExpire(this, EventArgs.Empty);
        }
    }
}
