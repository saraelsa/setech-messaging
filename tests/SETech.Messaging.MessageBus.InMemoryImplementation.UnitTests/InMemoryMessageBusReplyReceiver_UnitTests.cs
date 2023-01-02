using SETech.Messaging.MessageBus.InMemoryImplementation.Bus;
using SETech.Messaging.MessageBus.InMemoryImplementation.Implementations;
using SETech.Messaging.MessageBus.Primitives;

namespace SETech.Messaging.MessageBus.InMemoryImplementation.UnitTests;

public class InMemoryMessageBusReplyReceiver_UnitTests
{
    private InMemoryQueue<TestCorrelatedMessagePayload> testQueue = new (new InMemoryQueueOptions());

    private InMemoryMessageBusReplyReceiver<TestCorrelatedMessagePayload> testReplyReceiver;

    private TestCorrelatedMessagePayload testPayload1 = new () { CorrelationId = "1" };
    private TestCorrelatedMessagePayload testPayload2 = new () { CorrelationId = "2" };

    private BusMessage<TestCorrelatedMessagePayload> testMessage1;
    private BusMessage<TestCorrelatedMessagePayload> testMessage2;

    private class TestCorrelatedMessagePayload : ICorrelatedMessagePayload
    {
        public required string CorrelationId { get; set; }
    }

    public InMemoryMessageBusReplyReceiver_UnitTests()
    {
        testReplyReceiver = new InMemoryMessageBusReplyReceiver<TestCorrelatedMessagePayload>(testQueue);

        testMessage1 = new BusMessage<TestCorrelatedMessagePayload>() { Payload = testPayload1 };
        testMessage2 = new BusMessage<TestCorrelatedMessagePayload>() { Payload = testPayload2 };
    }

    [Fact]
    public async Task ReceiveReply_SingleMessage_Works()
    {
        testQueue.Publish(testMessage1);

        var receivedMessageTask = testReplyReceiver.ReceiveReplyAsync(testPayload1.CorrelationId);

        Assert.True(receivedMessageTask.IsCompletedSuccessfully);
        Assert.Equal(testPayload1, (await receivedMessageTask).Payload);
    }

    [Fact]
    public async Task ReceiveReply_TwoMessages_SameOrder_Works()
    {
        testQueue.Publish(testMessage1);
        testQueue.Publish(testMessage2);

        var receivedMessageTask1 = testReplyReceiver.ReceiveReplyAsync(testPayload1.CorrelationId);
        var receivedMessageTask2 = testReplyReceiver.ReceiveReplyAsync(testPayload2.CorrelationId);

        Assert.True(receivedMessageTask1.IsCompletedSuccessfully);
        Assert.Equal(testPayload1, (await receivedMessageTask1).Payload);

        Assert.True(receivedMessageTask2.IsCompletedSuccessfully);
        Assert.Equal(testPayload2, (await receivedMessageTask2).Payload); 
    }

    [Fact]
    public async Task ReceiveReply_TwoMessages_ReverseOrder_Works()
    {
        testQueue.Publish(testMessage1);
        testQueue.Publish(testMessage2);

        var receivedMessageTask2 = testReplyReceiver.ReceiveReplyAsync(testPayload2.CorrelationId);
        var receivedMessageTask1 = testReplyReceiver.ReceiveReplyAsync(testPayload1.CorrelationId);

        Assert.True(receivedMessageTask1.IsCompletedSuccessfully);
        Assert.Equal(testPayload1, (await receivedMessageTask1).Payload);

        Assert.True(receivedMessageTask2.IsCompletedSuccessfully);
        Assert.Equal(testPayload2, (await receivedMessageTask2).Payload); 
    }
}
