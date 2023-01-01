using SETech.Messaging.MessageBus.InMemoryImplementation.Bus;
using SETech.Messaging.MessageBus.Primitives;

namespace SETech.Messaging.MessageBus.InMemoryImplementation.UnitTests;

public sealed class InMemoryQueue_UnitTests
{
    private InMemoryQueue<object> testQueue = new (new InMemoryQueueOptions());

    (BusMessage<object>, object) CreateTestMessage()
    {
        object payload = new ();
        BusMessage<object> message = new () { Payload = payload };

        return (message, payload);
    }

    [Fact]
    public void SimpleSendAndReceive_SendFirst_Works()
    {
        var (message, payload) = CreateTestMessage();

        ReceivedBusMessage<object>? receivedMessage = null;

        testQueue.Publish(message);

        testQueue.Receive((message, actions) =>
        {
            receivedMessage = message;
            actions.Complete();
        });

        Assert.NotNull(receivedMessage);
        Assert.Equal(payload, receivedMessage.Payload);
    }

    [Fact]
    public void SimpleSendAndReceive_ReceiveFirst_Works()
    {
        var (message, payload) = CreateTestMessage();

        ReceivedBusMessage<object>? receivedMessage = null;

        testQueue.Receive((message, actions) =>
        {
            receivedMessage = message;
            actions.Complete();
        });

        testQueue.Publish(message);

        Assert.NotNull(receivedMessage);
        Assert.Equal(payload, receivedMessage.Payload);
    }
}
