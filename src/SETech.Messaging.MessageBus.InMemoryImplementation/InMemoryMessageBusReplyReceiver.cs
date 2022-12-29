using SETech.Messaging.MessageBus.Primitives;
using SETech.Messaging.MessageBus.Receiver;

namespace SETech.Messaging.MessageBus.InMemoryImplementation;

public class InMemoryMessageBusReplyReceiver<TPayload> : IMessageBusReplyReceiver<TPayload>
    where TPayload : ICorrelatedMessagePayload
{
    public Task<ReceivedBusMessage<TPayload>> ReceiveReplyAsync
    (
        string correlationId,
        CancellationToken cancellationToken = default
    )
    {
        throw new NotImplementedException();
    }
}
