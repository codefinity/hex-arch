using HexArch.Events.Payment;
using HexArch.Messaging.RabbitMQ.Transport;
using HexArch.Messaging.RabbitMQ.Transport.Topology;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.DeactivateUser;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HexArch.Listener.RabbitMQ.Consumers
{
    /// <summary>Deactivates a user in response to a fraud alert from the payment context.</summary>
    internal sealed class FraudDetectedConsumer : QueueConsumer<FraudDetected>
    {
        private const string DeactivationReason = "Fraud detected.";

        private readonly RabbitMqOptions options;

        public FraudDetectedConsumer(
            RabbitMqOptions options,
            IServiceScopeFactory scopeFactory,
            ILogger<FraudDetectedConsumer> logger)
            : base(options, scopeFactory, logger)
        {
            this.options = options;
        }

        protected override string QueueName => MessagingTopology.DeactivateOnFraudQueue(options.ExchangePrefix);

        protected override async Task Handle(FraudDetected domainEvent, IServiceProvider scope, CancellationToken cancellationToken)
        {
            var handler = scope.GetRequiredService<IDeactivateUserCommandHandler>();

            var command = new DeactivateUserCommand(domainEvent.UserId, DeactivationReason);
            var result = await handler.Handle(command, cancellationToken);

            if (!result.Success)
            {
                // Throwing routes this through QueueConsumer's retry path rather than acking a fraud
                // alert away. A FraudDetected can legitimately race the user's own row (e.g. arriving
                // before registration commits), and 30s of retry costs nothing; an account that is
                // still unresolved after five attempts lands on the dead-letter queue where an
                // operator can see it, which is far better than silently dropping a fraud signal.
                throw new InvalidOperationException(
                    $"Could not deactivate user {domainEvent.UserId}: {string.Join("; ", result.Errors)}");
            }
        }
    }
}
