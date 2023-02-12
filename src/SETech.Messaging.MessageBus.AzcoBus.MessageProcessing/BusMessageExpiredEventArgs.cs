using System.ComponentModel;
using SETech.Messaging.MessageBus.AzcoBus.MessageEntities;

namespace SETech.Messaging.MessageBus.AzcoBus.MessageProcessing;

public sealed class BusMessageExpiredEventArgs : CancelEventArgs
{
    public BusMessageExpiredEventArgs(ReceivedBusMessage message)
    {
        Message = message;
    }

    public ReceivedBusMessage Message { get; private init; }
}
