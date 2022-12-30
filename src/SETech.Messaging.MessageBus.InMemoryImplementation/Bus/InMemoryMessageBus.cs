namespace SETech.Messaging.MessageBus.InMemoryImplementation.Bus;

/// <summary>The <see cref="InMemoryMessageBus"/> holds queues, topics and subscriptions in-memory.</summary>
/// <remarks>
///     To create an <see cref="InMemoryMessageBus"/>, use <see cref="FluentBuilder.InMemoryMessageBusBuilder"/>.
/// </remarks>
public class InMemoryMessageBus
{
    /// <summary>The queues and topics in this <see cref="InMemoryMessageBus"/>.</summary>
    public IReadOnlyDictionary<string, object> Queues { get; protected init; }

    /// <summary>Creates an <see cref="InMemoryMessageBus"/> with the given queues, topics and subscriptions.</summary>
    /// <remarks>
    ///     To create an <see cref="InMemoryMessageBus"/>, use <see cref="FluentBuilder.InMemoryMessageBusBuilder"/>.
    /// </remarks>
    /// <param name="queues">
    ///     The queues, topics and subscriptions to create this <see cref="InMemoryMessageBus"/> with.
    /// </param>
    public InMemoryMessageBus(IDictionary<string, object> queues)
    {
        Queues = queues.AsReadOnly();
    }
}
