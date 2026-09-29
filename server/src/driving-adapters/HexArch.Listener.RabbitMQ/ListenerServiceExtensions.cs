using HexArch.Listener.RabbitMQ.Consumers;
using HexArch.Messaging.RabbitMQ.Transport;

using Microsoft.Extensions.DependencyInjection;

namespace HexArch.Listener.RabbitMQ
{
    public static class ListenerServiceExtensions
    {
        public static IServiceCollection AddHexArchRabbitMqListener(this IServiceCollection services, RabbitMqOptions options)
        {
            services.AddHexArchRabbitMqTransport(options);

            // Drains identity-access.deactivate-on-fraud. Same stance as the transport and the
            // publishing adapter: a broker that is down at startup degrades this feature rather
            // than blocking the host.
            services.AddHostedService<FraudDetectedConsumer>();

            return services;
        }
    }
}
