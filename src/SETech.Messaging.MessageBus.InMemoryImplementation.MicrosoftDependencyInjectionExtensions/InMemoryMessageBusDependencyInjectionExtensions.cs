using SETech.Messaging.MessageBus.Client;
using SETech.Messaging.MessageBus.InMemoryImplementation.Bus;
using SETech.Messaging.MessageBus.InMemoryImplementation.Bus.FluentBuilder;
using SETech.Messaging.MessageBus.InMemoryImplementation.Implementations;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>This static class contains extension methods on <see cref="IServiceCollection" />.</summary>
public static class InMemoryMessageBusDependencyInjectionExtensions
{
    /// <summary>
    ///     Creates an <see cref="InMemoryMessageBus"/> and adds a corresponding <see cref="IMessageBusClient"/> to the
    ///     service collection.
    /// </summary>
    /// <remarks>The service is added as a singleton.</remarks>
    /// <param name="serviceCollection">The service collection to add the message bus client to.</param>
    /// <param name="messageBusBuilderAction">The action that sets up the <see cref="InMemoryMessageBus"/>.</param>
    public static IServiceCollection AddInMemoryMessageBusClient
    (
        this IServiceCollection serviceCollection,
        Action<InMemoryMessageBusBuilder> messageBusBuilderAction
    )
    {
        InMemoryMessageBusBuilder builder = new ();
        messageBusBuilderAction(builder);

        InMemoryMessageBus bus = builder.Build();
        InMemoryMessageBusClient client = new InMemoryMessageBusClient(bus);

        serviceCollection.AddSingleton<IMessageBusClient>(client);

        return serviceCollection;
    }
}
