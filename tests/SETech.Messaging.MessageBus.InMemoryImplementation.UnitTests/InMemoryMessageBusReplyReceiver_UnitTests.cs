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
    public void ReceiveReply_TwoMessages_SameOrder_Works()
    {
        testQueue.Publish(testMessage1);
        testQueue.Publish(testMessage2);

        var receivedMessageTask1 = testReplyReceiver.ReceiveReplyAsync(testPayload1.CorrelationId);
        var receivedMessageTask2 = testReplyReceiver.ReceiveReplyAsync(testPayload2.CorrelationId);

        Assert.True(receivedMessageTask1.IsCompletedSuccessfully);
        Assert.Equal(testPayload1, receivedMessageTask1.GetAwaiter().GetResult().Payload);

        Assert.True(receivedMessageTask2.IsCompletedSuccessfully);
        Assert.Equal(testPayload2, receivedMessageTask2.GetAwaiter().GetResult().Payload); 
    }

    [Fact]
    public void ReceiveReply_TwoMessages_ReverseOrder_Works()
    {
        testQueue.Publish(testMessage1);
        testQueue.Publish(testMessage2);

        var receivedMessageTask2 = testReplyReceiver.ReceiveReplyAsync(testPayload2.CorrelationId);
        var receivedMessageTask1 = testReplyReceiver.ReceiveReplyAsync(testPayload1.CorrelationId);

        Assert.True(receivedMessageTask1.IsCompletedSuccessfully);
        Assert.Equal(testPayload1, receivedMessageTask1.GetAwaiter().GetResult().Payload);

        Assert.True(receivedMessageTask2.IsCompletedSuccessfully);
        Assert.Equal(testPayload2, receivedMessageTask2.GetAwaiter().GetResult().Payload); 
    }

    [Fact]
    public async Task ReceiveReply_OperationCancelledBeforeRequest_ThrowsOperationCancelledException()
    {
        CancellationTokenSource cts = new ();
        cts.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => testReplyReceiver.ReceiveReplyAsync(correlationId: "", cts.Token));
    }

    [Fact]
    public void ReceiveReply_OperationCancelledAfterRequest_DoesNotReceive()
    {
        CancellationTokenSource cts = new ();

        var receivedMessageTask = testReplyReceiver.ReceiveReplyAsync(testPayload1.CorrelationId, cts.Token);

        cts.Cancel();

        testQueue.Publish(testMessage1);

        Assert.True(receivedMessageTask.IsCanceled);

        var peekedMessage = testQueue.Peek(fromSequenceNumber: 0);

        Assert.NotNull(peekedMessage);
        Assert.False(peekedMessage.Deferred);
    }
}
