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
    public void SendAndReceive_SingleMessage_SendFirst_Works()
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
    public void SendAndReceive_SingleMessage_ReceiveFirst_Works()
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

    [Fact]
    public void SendAndReceive_MultipleMessages_Works()
    {
        var (message1, payload1) = CreateTestMessage();
        var (message2, payload2) = CreateTestMessage();

        ReceivedBusMessage<object>? receivedMessage1 = null;
        ReceivedBusMessage<object>? receivedMessage2 = null;

        testQueue.Publish(message1);
        testQueue.Publish(message2);

        testQueue.Receive((message, actions) =>
        {
            receivedMessage1 = message;
            actions.Complete();
        });

        testQueue.Receive((message, actions) =>
        {
            receivedMessage2 = message;
            actions.Complete();
        });

        Assert.NotNull(message1);
        Assert.Equal(payload1, message1.Payload);
  
        Assert.NotNull(message2);
        Assert.Equal(payload2, message2.Payload);
    }
}
