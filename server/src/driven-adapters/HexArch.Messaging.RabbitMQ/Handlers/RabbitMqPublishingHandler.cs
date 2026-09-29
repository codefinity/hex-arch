using System.Text.Json;
using System.Text.Json.Serialization;

using HexArch.Events;
using HexArch.Messaging.RabbitMQ.Transport;
using HexArch.Messaging.RabbitMQ.Transport.Topology;

using Microsoft.Extensions.Logging;

namespace HexArch.Messaging.RabbitMQ.Handlers
{
    /// <summary>
    /// Mirrors every dispatched domain event onto RabbitMQ. Registered as an open generic, so it
    /// closes over whatever event the dispatcher asks for and never needs a per-event registration.
    /// </summary>
    internal sealed class RabbitMqPublishingHandler<TEvent> : IEventHandler<TEvent>
        where TEvent : IDomainEvent
    {
        private const int EnvelopeVersion = 1;

        // One static field per closed generic, so the reflection is paid once per event type.
        private static readonly string RoutingKey = EventContracts.For<TEvent>();

        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false
        };

        private readonly IRabbitMqPublisher publisher;
        private readonly RabbitMqOptions options;
        private readonly ILogger<RabbitMqPublishingHandler<TEvent>> logger;

        public RabbitMqPublishingHandler(
            IRabbitMqPublisher publisher,
            RabbitMqOptions options,
            ILogger<RabbitMqPublishingHandler<TEvent>> logger)
        {
            this.publisher = publisher;
            this.options = options;
            this.logger = logger;
        }

        public async Task Handle(TEvent domainEvent, CancellationToken cancellationToken = default)
        {
            // IDomainEvent carries no id, so mint one per publish and put it in both the AMQP
            // MessageId and the body (a non-AMQP consumer may only ever see the body).
            var eventId = Guid.NewGuid().ToString();

            try
            {
                var envelope = new EventEnvelope<TEvent>(
                    EventId: eventId,
                    Type: RoutingKey,
                    OccurredOnUtc: domainEvent.OccurredOnUtc,
                    Version: EnvelopeVersion,
                    Source: options.ApplicationName,
                    Data: domainEvent);

                // TEvent is the closed concrete record here, so every property serializes.
                // Widening the static type to IDomainEvent would emit OccurredOnUtc only.
                var body = JsonSerializer.SerializeToUtf8Bytes(envelope, JsonOptions);

                // Deliberately NOT the request token: the write is already committed and the caller
                // may have disconnected. Bound the wait instead, so a wedged broker cannot stall
                // the response beyond PublishTimeoutSeconds.
                using var timeout = new CancellationTokenSource(
                    TimeSpan.FromSeconds(options.PublishTimeoutSeconds));

                await publisher.Publish(RoutingKey, eventId, domainEvent.OccurredOnUtc, body, timeout.Token);
            }
            catch (Exception exception)
            {
                // Fire-and-forget: the write already succeeded, so a broker problem must not fail a
                // request that worked. Same stance as UserRegisteredProjectionHandler / WelcomeEmailHandler.
                logger.LogError(exception,
                    "Failed to publish {RoutingKey} ({EventId}) to RabbitMQ.", RoutingKey, eventId);
            }
        }
    }
}
