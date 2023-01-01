using SETech.Messaging.MessageBus.InMemoryImplementation.Bus;
using SETech.Messaging.MessageBus.Primitives;

namespace SETech.Messaging.MessageBus.InMemoryImplementation.UnitTests;

public class InMemoryTopic_UnitTests
{
    private InMemoryQueue<object> testSubscription1 = new (new InMemoryQueueOptions());
    private InMemoryQueue<object> testSubscription2 = new (new InMemoryQueueOptions());

    private InMemoryTopic<object> testTopic;

    public InMemoryTopic_UnitTests()
    {
        Dictionary<string, InMemoryQueue<object>> subscriptions = new ()
        {
            { nameof(testSubscription1), testSubscription1 },
            { nameof(testSubscription2), testSubscription2 }
        };

        testTopic = new (subscriptions, new InMemoryQueueOptions());
    }

    private (BusMessage<object>, object) CreateTestMessage()
    {
        object payload = new ();
        BusMessage<object> message = new () { Payload = payload };

        return (message, payload);
    }

    [Fact]
    public void Publish_ReceivesAndSendsToSubscriptions()
    {
        var (message, payload) = CreateTestMessage();

        testTopic.Publish(message);

        BusMessage<object>? messageInTopic = testTopic.Peek(fromSequenceNumber: 0);

        Assert.Null(messageInTopic);

        BusMessage<object>? messageInSubscription1 = testSubscription1.Peek(fromSequenceNumber: 0);
        BusMessage<object>? messageInSubscription2 = testSubscription2.Peek(fromSequenceNumber: 0);

        Assert.NotNull(messageInSubscription1);
        Assert.Equal(payload, messageInSubscription1.Payload);

        Assert.NotNull(messageInSubscription2);
        Assert.Equal(payload, messageInSubscription2.Payload);
    }
}
