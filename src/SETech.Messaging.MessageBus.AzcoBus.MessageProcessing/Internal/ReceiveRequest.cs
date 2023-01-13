using SETech.Messaging.MessageBus.AzcoBus.MessageEntities;

namespace SETech.Messaging.MessageBus.AzcoBus.MessageProcessing.Internal;

/// <summary>Represents a request to receive a message from a queue or subscription</summary>
public class ReceiveRequest
{
    public required TaskCompletionSource<ReceivedBusMessage> TaskCompletionSource { get; init; }
    public required CancellationToken CancellationToken { get; init; }
}
