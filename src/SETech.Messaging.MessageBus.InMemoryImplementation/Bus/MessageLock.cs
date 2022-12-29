using SETech.Messaging.MessageBus.Receiver;

namespace SETech.Messaging.MessageBus.InMemoryImplementation.Bus;

public sealed class MessageLock
{
    public TimeSpan LockDuration { get; private init; }

    public bool Expired => _expired;

    public event EventHandler? OnExpire;
    
    private DateTimeOffset _expiresOn;
    private bool _expired = false;
    private bool _cleared = false;

    public MessageLock(TimeSpan lockDuration)
    {
        LockDuration = lockDuration;
        Renew();
        CheckForExpiry();
    }

    public Action CreateHandler(Action protectedAction) =>
        () =>
        {
            if (_expired)
                throw new LockExpiredException();

            if (_cleared)
                throw new InvalidOperationException();
            
            _cleared = true;

            protectedAction();
        };
    
    public void Renew()
    {
        _expiresOn = DateTimeOffset.Now + LockDuration;
    }

    private void CheckForExpiry()
    {
        if (DateTimeOffset.Now < _expiresOn)
        {
            Task.Delay(LockDuration).ContinueWith(_ => CheckForExpiry());
            return;
        }

        _expired = true;

        if (OnExpire != null)
            OnExpire(this, EventArgs.Empty);
    }
}
