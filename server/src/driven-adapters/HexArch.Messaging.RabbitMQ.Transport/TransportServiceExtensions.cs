using HexArch.Messaging.RabbitMQ.Transport.Topology;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace HexArch.Messaging.RabbitMQ.Transport
{
    public static class TransportServiceExtensions
    {
        public static IServiceCollection AddHexArchRabbitMqTransport(
            this IServiceCollection services, RabbitMqOptions options)
        {
            // TryAdd: both the publishing adapter and the listener adapter call this, and each
            // should be able to run on its own without the other.
            services.TryAddSingleton(options);

            // Creates the exchanges, queues and bindings on the broker if they are not already there.
            // Hosted services start with the host, so this does not delay the HTTP pipeline, and a
            // broker that is down degrades messaging instead of blocking startup.
            services.AddHostedService<RabbitMqTopologyInstaller>();

            return services;
        }
    }
}
