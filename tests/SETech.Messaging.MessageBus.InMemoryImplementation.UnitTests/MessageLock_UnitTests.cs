using SETech.Messaging.MessageBus.InMemoryImplementation.Bus;
using SETech.Messaging.MessageBus.Receiver;

namespace SETech.Messaging.MessageBus.InMemoryImplementation.UnitTests;

public class MessageLock_UnitTests
{
    [Fact]
    public void Handle_WithinInitialLockPeriod_Works()
    {
        MessageLock messageLock = new (lockDuration: TimeSpan.FromSeconds(5));

        bool handled = false;

        messageLock.CreateHandler(() => handled = true)();

        Assert.True(handled);
    }

    [Fact]
    public async Task Handle_WithinRenewedLockPeriod_Works()
    {
        MessageLock messageLock = new (lockDuration: TimeSpan.FromMilliseconds(50));

        bool handled = false;

        var handler = messageLock.CreateHandler(() => handled = true);

        await Task.Delay(25);

        messageLock.Renew();

        handler();

        Assert.True(messageLock.Cleared);
        Assert.False(messageLock.Expired);
        Assert.True(handled);
    }

    [Fact]
    public async Task Handle_LockExpired_ThrowsLockExpiredException()
    {
        MessageLock messageLock = new (lockDuration: TimeSpan.FromMilliseconds(10));

        var handler = messageLock.CreateHandler(() => { });

        await Task.Delay(20);

        Assert.True(messageLock.Expired);
        Assert.Throws<LockExpiredException>(() => handler());
        Assert.False(messageLock.Cleared);
    }

    [Fact]
    public void Handle_LockCleared_ThrowsInvalidOperationException()
    {
        MessageLock messageLock = new (lockDuration: TimeSpan.FromSeconds(5));

        var handler = messageLock.CreateHandler(() => { });

        handler();

        Assert.Throws<InvalidOperationException>(() => handler());
    }
}
