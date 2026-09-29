using System.Text.Json;

using HexArch.Events;
using HexArch.Messaging.RabbitMQ.Transport;
using HexArch.Messaging.RabbitMQ.Transport.Topology;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace HexArch.Listener.RabbitMQ.Consumers
{
    /// <summary>
    /// Owns the ack / retry / dead-letter protocol for draining one work queue, so a new consumer
    /// cannot get it subtly wrong — the same reason <see cref="MessagingTopology"/> builds a work
    /// queue's wiring in one place instead of leaving each subscriber to repeat it.
    ///
    /// Prefetch 1 and single-threaded dispatch serialise message handling on the one channel this
    /// class owns, which is what makes it safe to publish the retry hop on that same channel —
    /// <see cref="IChannel"/> is not thread-safe.
    /// </summary>
    internal abstract class QueueConsumer<TEvent> : BackgroundService
        where TEvent : IDomainEvent
    {
        private const string AttemptHeader = "x-hc-attempt";

        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        private readonly RabbitMqOptions options;
        private readonly IServiceScopeFactory scopeFactory;
        private readonly ILogger logger;

        protected QueueConsumer(RabbitMqOptions options, IServiceScopeFactory scopeFactory, ILogger logger)
        {
            this.options = options;
            this.scopeFactory = scopeFactory;
            this.logger = logger;
        }

        protected abstract string QueueName { get; }

        protected abstract Task Handle(TEvent domainEvent, IServiceProvider scope, CancellationToken cancellationToken);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Unlike RabbitMqTopologyInstaller, this loop never gives up: a consumer that stops
            // retrying after a broker outage is a silently broken feature, not a degraded one.
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ConsumeUntilCancelled(stoppingToken);
                    return;
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    return;
                }
                catch (Exception exception)
                {
                    logger.LogWarning(exception,
                        "Could not start consuming {QueueName} from RabbitMQ. Retrying in {Cooldown}s.",
                        QueueName, options.ConnectRetryCooldownSeconds);
                }

                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(options.ConnectRetryCooldownSeconds), stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    return;
                }
            }
        }

        private async Task ConsumeUntilCancelled(CancellationToken stoppingToken)
        {
            var factory = new ConnectionFactory
            {
                HostName = options.HostName,
                Port = options.Port,
                UserName = options.UserName,
                Password = options.Password,
                VirtualHost = options.VirtualHost,
                ClientProvidedName = $"{options.ApplicationName}-{QueueName}",
                AutomaticRecoveryEnabled = true,
                TopologyRecoveryEnabled = true,
                // One message in flight, dispatched on a single thread, so this consumer's own
                // retry publish never races a concurrent delivery on the same channel.
                ConsumerDispatchConcurrency = 1
            };

            IConnection? connection = null;
            IChannel? channel = null;

            try
            {
                connection = await factory.CreateConnectionAsync(cancellationToken: stoppingToken);
                channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

                await channel.BasicQosAsync(
                    prefetchSize: 0, prefetchCount: 1, global: false, cancellationToken: stoppingToken);

                var consumer = new AsyncEventingBasicConsumer(channel);
                consumer.ReceivedAsync += (_, args) => OnReceived(channel, args, stoppingToken);

                await channel.BasicConsumeAsync(
                    queue: QueueName, autoAck: false, consumer: consumer, cancellationToken: stoppingToken);

                logger.LogInformation("Consuming {QueueName} from RabbitMQ.", QueueName);

                await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken);
            }
            finally
            {
                await CloseQuietly(channel, connection);
            }
        }

        private async Task OnReceived(IChannel channel, BasicDeliverEventArgs args, CancellationToken cancellationToken)
        {
            EventEnvelope<TEvent>? envelope;
            try
            {
                envelope = JsonSerializer.Deserialize<EventEnvelope<TEvent>>(args.Body.Span, JsonOptions);
            }
            catch (JsonException exception)
            {
                // Malformed JSON will never parse no matter how many times it's retried, so it goes
                // straight to the dead-letter queue rather than round-tripping the retry queue first.
                logger.LogError(exception,
                    "Could not deserialize a message from {QueueName}; dead-lettering.", QueueName);
                await channel.BasicNackAsync(
                    deliveryTag: args.DeliveryTag, multiple: false, requeue: false, cancellationToken: cancellationToken);
                return;
            }

            if (envelope is null)
            {
                logger.LogError("Deserialized a null envelope from {QueueName}; dead-lettering.", QueueName);
                await channel.BasicNackAsync(
                    deliveryTag: args.DeliveryTag, multiple: false, requeue: false, cancellationToken: cancellationToken);
                return;
            }

            try
            {
                // A scope per message, since the application handler and its dependencies (e.g. the
                // EF Core DbContext) are scoped.
                using var scope = scopeFactory.CreateScope();
                await Handle(envelope.Data, scope.ServiceProvider, cancellationToken);
                await channel.BasicAckAsync(deliveryTag: args.DeliveryTag, multiple: false, cancellationToken: cancellationToken);
            }
            catch (Exception exception)
            {
                await RetryOrDeadLetter(channel, args, exception, cancellationToken);
            }
        }

        private async Task RetryOrDeadLetter(
            IChannel channel, BasicDeliverEventArgs args, Exception exception, CancellationToken cancellationToken)
        {
            var attempt = ReadAttempt(args.BasicProperties) + 1;

            if (attempt > MessagingTopology.DeliveryLimit)
            {
                logger.LogError(exception,
                    "Giving up on a message from {QueueName} after {Attempt} attempt(s); dead-lettering.",
                    QueueName, attempt);
                await channel.BasicNackAsync(
                    deliveryTag: args.DeliveryTag, multiple: false, requeue: false, cancellationToken: cancellationToken);
                return;
            }

            logger.LogWarning(exception,
                "Failed to handle a message from {QueueName} (attempt {Attempt}); retrying shortly.",
                QueueName, attempt);

            // Republished as a brand-new message via the default exchange, keyed by the retry queue's
            // own name; that queue's TTL dead-letters it back to the work queue after the delay.
            // Republishing creates a new message, which resets the broker's own x-delivery-limit
            // counter — that's why the attempt count has to travel in a header instead.
            var retryProperties = new BasicProperties(args.BasicProperties)
            {
                Headers = new Dictionary<string, object?> { [AttemptHeader] = attempt }
            };

            await channel.BasicPublishAsync(
                exchange: string.Empty,
                routingKey: MessagingTopology.RetryQueue(QueueName),
                mandatory: false,
                basicProperties: retryProperties,
                body: args.Body,
                cancellationToken: cancellationToken);

            await channel.BasicAckAsync(deliveryTag: args.DeliveryTag, multiple: false, cancellationToken: cancellationToken);
        }

        private static int ReadAttempt(IReadOnlyBasicProperties properties)
        {
            if (properties.Headers is null || !properties.Headers.TryGetValue(AttemptHeader, out var value))
            {
                return 0;
            }

            return value switch
            {
                int i => i,
                long l => (int)l,
                byte[] bytes => int.Parse(System.Text.Encoding.UTF8.GetString(bytes)),
                _ => 0
            };
        }

        private async Task CloseQuietly(IChannel? channel, IConnection? connection)
        {
            try { if (channel is not null) await channel.DisposeAsync(); }
            catch (Exception e) { logger.LogDebug(e, "Ignored error disposing the {QueueName} consumer channel.", QueueName); }

            try { if (connection is not null) await connection.DisposeAsync(); }
            catch (Exception e) { logger.LogDebug(e, "Ignored error disposing the {QueueName} consumer connection.", QueueName); }
        }
    }
}
