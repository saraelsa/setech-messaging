using SETech.Messaging.MessageBus.AzcoBus.MessageProcessing.Ports;

namespace SETech.Messaging.MessageBus.AzcoBus.MessageProcessing.Internal;

public class DelayedCallbackService : IDelayedCallbackService
{
    public virtual IDisposable ExecuteCallbackAfter(Action callback, TimeSpan delay)
    {
        return new Timer(
            callback: _ => callback(),
            state: null,
            dueTime: delay,
            period: Timeout.InfiniteTimeSpan);
    }

    public virtual IDisposable ExecuteCallbackOn(Action callback, DateTimeOffset time)
    {
        return ExecuteCallbackAfter(callback, time - DateTimeOffset.Now);
    }
}
