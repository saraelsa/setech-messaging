using SETech.Messaging.MessageBus.Client;
using SETech.Messaging.MessageBus.Primitives;
using SETech.Messaging.MessageBus.Receiver;
using SETech.Messaging.MessageBus.Sender;

namespace SETech.Messaging.MessageBus.InMemoryImplementation;

public class InMemoryMessageBusClient : IMessageBusClient
{
    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public IMessageBusReceiver<TPayload> CreateReceiver<TPayload>(string queueName)
    {
        throw new NotImplementedException();
    }

    public IMessageBusReceiver<TPayload> CreateReceiver<TPayload>(string topicName, string subscriptionName)
    {
        throw new NotImplementedException();
    }

    public IMessageBusReplyReceiver<TPayload> CreateReplyReceiver<TPayload>(string queueName)
        where TPayload : ICorrelatedMessagePayload
    {
        throw new NotImplementedException();
    }

    public IMessageBusSender<TPayload> CreateSender<TPayload>(string queueName)
    {
        throw new NotImplementedException();
    }
}
