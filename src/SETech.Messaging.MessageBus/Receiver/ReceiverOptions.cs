namespace SETech.Messaging.MessageBus.Receiver;

/// <summary>The options that can be configured while creating a <see cref="IMessageBusReceiver{TPayload}"/>.</summary>
public class ReceiverOptions
{
    /// <summary>The mode to receive messages with. This controls how messages are settled.</summary>
    public ReceiveMode ReceiveMode { get; set; } = ReceiveMode.PeekLock;
}
