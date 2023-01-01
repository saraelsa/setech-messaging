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

        Assert.NotNull(receivedMessage1);
        Assert.Equal(payload1, receivedMessage1.Payload);
  
        Assert.NotNull(receivedMessage2);
        Assert.Equal(payload2, receivedMessage2.Payload);
    }

    [Fact]
    public void Peek_NoAvailableMessages_ReturnsNull()
    {
        var receivedMessage = testQueue.Peek(0);

        Assert.Null(receivedMessage);
    }

    [Fact]
    public void Peek_SingleMessage_Works()
    {
        var (message, payload) = CreateTestMessage();

        testQueue.Publish(message);

        var receivedMessage = testQueue.Peek(fromSequenceNumber: 0);

        Assert.NotNull(receivedMessage);
        Assert.Equal(payload, receivedMessage.Payload);
    }

    [Fact]
    public void Peek_MultipleMessages_SkipByMinimumSequenceNumber_Works()
    {
        var (message1, payload1) = CreateTestMessage();
        var (message2, payload2) = CreateTestMessage();

        testQueue.Publish(message1);
        testQueue.Publish(message2);

        var receivedMessage = testQueue.Peek(fromSequenceNumber: 1);

        Assert.NotNull(receivedMessage);
        Assert.Equal(payload2, receivedMessage.Payload);
    }

    [Fact]
    public void PeekMany_MultipleMessages_NoAvailableMessages_ReturnsEmptyCollection()
    {
        var receivedMessages = testQueue.PeekMany(fromSequenceNumber: 0, maximumResults: 1);

        Assert.Empty(receivedMessages);
    }

    [Fact]
    public void PeekMany_MultipleMessages_AllMessages_Works()
    {
        var (message1, payload1) = CreateTestMessage();
        var (message2, payload2) = CreateTestMessage();

        testQueue.Publish(message1);
        testQueue.Publish(message2);

        var receivedMessages = testQueue.PeekMany(fromSequenceNumber: 0, maximumResults: 2);

        Assert.Equal(2, receivedMessages.Count);
        Assert.Equal(payload1, receivedMessages[0].Payload);
        Assert.Equal(payload2, receivedMessages[1].Payload);
    }

    [Fact]
    public void PeekMany_MultipleMessages_SkipByMinimumSequenceNumber_Works()
    {
        var (message1, payload1) = CreateTestMessage();
        var (message2, payload2) = CreateTestMessage();
        var (message3, payload3) = CreateTestMessage();

        testQueue.Publish(message1);
        testQueue.Publish(message2);
        testQueue.Publish(message3);

        var receivedMessages = testQueue.PeekMany(fromSequenceNumber: 1, maximumResults: 2);

        Assert.Equal(2, receivedMessages.Count);
        Assert.Equal(payload2, receivedMessages[0].Payload);
        Assert.Equal(payload3, receivedMessages[1].Payload);
    }
}
