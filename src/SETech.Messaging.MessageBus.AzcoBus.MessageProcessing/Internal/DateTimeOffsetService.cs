using SETech.Messaging.MessageBus.AzcoBus.MessageProcessing.Ports;

namespace SETech.Messaging.MessageBus.AzcoBus.MessageProcessing.Internal;

public class DateTimeOffsetService : IDateTimeOffsetService
{
    public virtual DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
