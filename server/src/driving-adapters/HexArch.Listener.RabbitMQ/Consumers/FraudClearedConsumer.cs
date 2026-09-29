using HexArch.Events.Payment;
using HexArch.Messaging.RabbitMQ.Transport;
using HexArch.Messaging.RabbitMQ.Transport.Topology;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.ReactivateUser;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HexArch.Listener.RabbitMQ.Consumers
{
    /// <summary>Reactivates a user when the payment context withdraws an earlier fraud alert.</summary>
    internal sealed class FraudClearedConsumer : QueueConsumer<FraudCleared>
    {
        private const string ReactivationReason = "Fraud cleared.";

        private readonly RabbitMqOptions options;

        public FraudClearedConsumer(
            RabbitMqOptions options,
            IServiceScopeFactory scopeFactory,
            ILogger<FraudClearedConsumer> logger)
            : base(options, scopeFactory, logger)
        {
            this.options = options;
        }

        protected override string QueueName => MessagingTopology.ReactivateOnFraudClearedQueue(options.ExchangePrefix);

        protected override async Task Handle(FraudCleared domainEvent, IServiceProvider scope, CancellationToken cancellationToken)
        {
            var handler = scope.GetRequiredService<IReactivateUserCommandHandler>();

            // Conditional: clearing a fraud alert must not also lift a suspension an administrator
            // made for some other reason. The use case treats that case as a successful no-op.
            var command = new ReactivateUserCommand(
                domainEvent.UserId, ReactivationReason, OnlyIfDeactivatedFor: FraudDetectedConsumer.DeactivationReason);
            var result = await handler.Handle(command, cancellationToken);

            if (!result.Success)
            {
                // Same stance as FraudDetectedConsumer: retry, then dead-letter where an operator can
                // see it, rather than ack the event away.
                throw new InvalidOperationException(
                    $"Could not reactivate user {domainEvent.UserId}: {string.Join("; ", result.Errors)}");
            }
        }
    }
}
