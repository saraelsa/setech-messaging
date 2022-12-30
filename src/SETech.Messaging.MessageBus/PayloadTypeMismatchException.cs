namespace SETech.Messaging.MessageBus;

/// <summary>
///     The <see cref="PayloadTypeMismatchException"/> indicates that the queue, topic or subscription can not use the
///     requested payload type.
/// </summary>
public class PayloadTypeMismatchException : Exception
{
    public PayloadTypeMismatchException()
        : base("The queue, topic or subscription can not use the requested payload type.") { }
}
