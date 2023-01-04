using Moq;
using SETech.Messaging.MessageBus.Dispatcher;
using SETech.Messaging.MessageBus.Primitives;
using SETech.Messaging.MessageBus.Sender;

namespace SETech.Messaging.MessageBus.UnitTests;

public sealed class MessageDispatcher_UnitTests
{
    public class TestPayload1 { }
    public class TestPayload2 { }

    [Fact]
    public void DispatchMessage_MultipleSenders_Works()
    {
        Mock<IMessageBusSender<TestPayload1>> mockSender1 = new ();
        Mock<IMessageBusSender<TestPayload2>> mockSender2 = new ();

        TestPayload1 testPayload1 = new ();
        TestPayload2 testPayload2 = new ();

        CancellationToken cancellationToken = default;

        mockSender1
            .Setup(sender => sender.SendMessageAsync
            (
                It.IsAny<BusMessage<TestPayload1>>(),
                It.IsAny<CancellationToken>()
            )).Returns(Task.CompletedTask);
        
        mockSender2
            .Setup(sender => sender.SendMessageAsync
            (
                It.IsAny<BusMessage<TestPayload2>>(),
                It.IsAny<CancellationToken>()
            )).Returns(Task.CompletedTask);

        MessageDispatcher dispatcher = new (new object[] { mockSender1.Object, mockSender2.Object });

        dispatcher.SendMessageAsync(testPayload1, cancellationToken);
        dispatcher.SendMessageAsync(testPayload2, cancellationToken);

        mockSender1.Verify(sender => sender.SendMessageAsync
        (
            It.IsAny<BusMessage<TestPayload1>>(),
            It.Is<CancellationToken>(c => c == cancellationToken)
        ));

        mockSender1.VerifyNoOtherCalls();

        mockSender2.Verify(sender => sender.SendMessageAsync
        (
            It.IsAny<BusMessage<TestPayload2>>(),
            It.Is<CancellationToken>(c => c == cancellationToken)
        ));

        mockSender2.VerifyNoOtherCalls();
    }
}