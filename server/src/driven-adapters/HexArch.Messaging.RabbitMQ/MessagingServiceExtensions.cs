using HexArch.Events;
using HexArch.Messaging.RabbitMQ.Handlers;
using HexArch.Messaging.RabbitMQ.Transport;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace HexArch.Messaging.RabbitMQ
{
    public static class MessagingServiceExtensions
    {
        public static IServiceCollection AddHexArchMessaging(this IServiceCollection services, RabbitMqOptions options)
        {
            services.TryAddSingleton(options);

            // Singleton: owns the one connection and channel for the process. The container disposes
            // it (asynchronously) at host shutdown.
            services.AddSingleton<IRabbitMqPublisher, RabbitMqPublisher>();

            // Creates the exchanges, queues and bindings on the broker if they are not already there.
            // Hosted services start with the host, so this does not delay the HTTP pipeline, and a
            // broker that is down degrades messaging instead of blocking startup.
            services.AddHexArchRabbitMqTransport(options);

            // Open generic: closes to RabbitMqPublishingHandler<T> for whatever T the dispatcher asks
            // for, so every current and future domain event reaches RabbitMQ with no per-event
            // registration. Sits alongside, not instead of, the concrete handlers.
            services.AddScoped(typeof(IEventHandler<>), typeof(RabbitMqPublishingHandler<>));

            return services;
        }
    }
}
