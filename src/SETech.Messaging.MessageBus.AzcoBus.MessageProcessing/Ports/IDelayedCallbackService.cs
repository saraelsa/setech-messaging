namespace SETech.Messaging.MessageBus.AzcoBus.MessageProcessing.Ports;

public interface IDelayedCallbackService
{
    /// <summary>Executes a callback at or after a specified time.</summary>
    /// <param name="callback">The callback to execute.</param>
    /// <param name="time">The time at which to execute the callback.</param>
    /// <returns>An <see cref="IDisposable"/> that can be used to cancel the callback's execution.</returns>
    IDisposable ExecuteCallbackOn(Action callback, DateTimeOffset time);

    /// <summary>Executes a callback after a specified delay.</summary>
    /// <param name="callback">The callback to execute.</param>
    /// <param name="delay">The delay after which to execute the callback.</param>
    /// <returns>An <see cref="IDisposable"/> that can be used to cancel the callback's execution.</returns>
    IDisposable ExecuteCallbackAfter(Action callback, TimeSpan delay);
}
