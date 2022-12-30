using SETech.Messaging.MessageBus.Primitives;
using SETech.Messaging.MessageBus.Receiver;

namespace SETech.Messaging.MessageBus.InMemoryImplementation.Implementations;

/// <summary>The <see cref="InMemoryMessageBusReplyReceiver{TPayload}"/> receives replies from an in-memory queue.</summary>
/// <inheritdoc />
public class InMemoryMessageBusReplyReceiver<TPayload> : IMessageBusReplyReceiver<TPayload>
    where TPayload : ICorrelatedMessagePayload
{
    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public Task<ReceivedBusMessage<TPayload>> ReceiveReplyAsync
    (
        string correlationId,
        CancellationToken cancellationToken = default
    )
    {
        throw new NotImplementedException();
    }
}
