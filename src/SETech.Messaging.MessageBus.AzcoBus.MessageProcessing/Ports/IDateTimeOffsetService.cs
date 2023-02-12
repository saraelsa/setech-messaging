namespace SETech.Messaging.MessageBus.AzcoBus.MessageProcessing.Ports;

public interface IDateTimeOffsetService
{
    DateTimeOffset UtcNow { get; }
}
