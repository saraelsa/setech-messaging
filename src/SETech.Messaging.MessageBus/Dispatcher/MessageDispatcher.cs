using SETech.Messaging.MessageBus.Primitives;
using SETech.Messaging.MessageBus.Sender;

namespace SETech.Messaging.MessageBus.Dispatcher;

/// <summary>
///     The <see cref="MessageDispatcher"/> dispatches requests to a registered <see cref="IMessageBusSender{TPayload}"/>
///     based on their payload type. It exposes the same methods as <see cref="IMessageBusSender{TPayload}"/> but allows
///     the payload type to be specified independently for each method call.
/// </summary>
public class MessageDispatcher
{
    /// <summary>
    ///     The registered <see cref="IMessageBusSender{TPayload}"/> objects keyed with their respective payload types.
    /// </summary>
    protected readonly IReadOnlyDictionary<Type, object> MessageBusSenders;

    /// <summary>
    ///     Creates a <see cref="MessageDispatcher"/> with the specified <see cref="IMessageBusSender{TPayload}"/> set.
    /// </summary>
    /// <param name="messageBusSenders">The <see cref="IMessageBusSender{TPayload}"/> set to dispatch requests to.</param>
    public MessageDispatcher(IEnumerable<object> messageBusSenders)
    {
        IDictionary<Type, object> senderDictionary = new Dictionary<Type, object>();

        foreach (object sender in messageBusSenders)
        {
            Type? senderInterface = sender.GetType().GetInterface(typeof(IMessageBusSender<>).Name);

            if (senderInterface is null)
                throw new ArgumentException("Message bus senders must implement IMessageBusSender<TPayload>.");

            Type payloadType = senderInterface.GetGenericArguments()[0];

            senderDictionary[payloadType] = sender;
        }

        MessageBusSenders = senderDictionary.AsReadOnly();
    }

    /// <inheritdoc cref="SendMessageAsync{TPayload}(TPayload, MessageDispatchOptions, CancellationToken)" />
    public Task SendMessageAsync<TPayload>(TPayload payload, CancellationToken cancellationToken = default) =>
        SendMessageAsync(payload, new MessageDispatchOptions(), cancellationToken);

    /// <inheritdoc cref="IMessageBusSender{TPayload}.SendMessageAsync" />
    /// <typeparam name="TPayload">The payload type to send.</typeparam>
    public async Task SendMessageAsync<TPayload>
    (
        TPayload payload,
        MessageDispatchOptions dispatchOptions,
        CancellationToken cancellationToken = default
    )
    {
        IMessageBusSender<TPayload> sender = GetSenderOrThrow<TPayload>();
        BusMessage<TPayload> message = CreateMessage(payload, dispatchOptions);

        await sender.SendMessageAsync(message, cancellationToken);
    }

    /// <inheritdoc
    ///     cref="ScheduleMessageAsync{TPayload}(TPayload, DateTimeOffset, MessageDispatchOptions, CancellationToken)"
    /// />
    public Task<long> ScheduleMessageAsync<TPayload>
    (
        TPayload payload,
        DateTimeOffset sendOn,
        CancellationToken cancellationToken = default
    ) => ScheduleMessageAsync(payload, sendOn, new MessageDispatchOptions(), cancellationToken);

    /// <inheritdoc cref="IMessageBusSender{TPayload}.ScheduleMessageAsync" />
    /// <typeparam name="TPayload">The payload type to send.</typeparam>
    public async Task<long> ScheduleMessageAsync<TPayload>
    (
        TPayload payload,
        DateTimeOffset sendOn,
        MessageDispatchOptions dispatchOptions,
        CancellationToken cancellationToken = default
    )
    {
        IMessageBusSender<TPayload> sender = GetSenderOrThrow<TPayload>();
        BusMessage<TPayload> message = CreateMessage(payload, dispatchOptions);

        long sequenceNumber = await sender.ScheduleMessageAsync(message, sendOn, cancellationToken);
        return sequenceNumber;
    }

    /// <inheritdoc cref="IMessageBusSender{TPayload}.CancelScheduledMessageAsync" />
    /// <typeparam name="TPayload">The payload type of the message to cancel.</typeparam>
    public async Task CancelScheduledMessageAsync<TPayload>
    (
        long sequenceNumber,
        CancellationToken cancellationToken = default
    )
    {
        IMessageBusSender<TPayload> sender = GetSenderOrThrow<TPayload>();

        await sender.CancelScheduledMessageAsync(sequenceNumber, cancellationToken);
    }

    protected static BusMessage<TPayload> CreateMessage<TPayload>
    (
        TPayload payload,
        MessageDispatchOptions dispatchOptions
    ) => new BusMessage<TPayload>() { Payload = payload, TimeToLive = dispatchOptions.TimeToLive };

    /// <summary>
    ///     Retrieves a <see cref="IMessageBusSender{TPayload}"/> from the registered senders based on its payload type.
    /// </summary>
    /// <typeparam name="TPayload">The payload type of the sender to retreive.</typeparam>
    /// <exception cref="InvalidOperationException">The specified payload type is not registered.</exception>
    protected IMessageBusSender<TPayload> GetSenderOrThrow<TPayload>()
    {
        Type payloadType = typeof(TPayload);

        if (!MessageBusSenders.TryGetValue(typeof(TPayload), out object? senderObject))
            throw new InvalidOperationException(
                $"This message dispatcher does not support dispatching messages with payload type {payloadType.Name}"
                + " because no sender with this type is registered to it."
            );

        IMessageBusSender<TPayload> sender = (senderObject as IMessageBusSender<TPayload>)!;

        return sender;
    }
}
