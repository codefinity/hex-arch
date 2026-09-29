using HexArch.Messaging.RabbitMQ.Transport;
using HexArch.Messaging.RabbitMQ.Transport.Topology;

using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace HexArch.Messaging.RabbitMQ
{
    /// <summary>
    /// Owns the process-wide RabbitMQ connection and publishing channel. Singleton: IConnection is
    /// expensive and thread-safe, IChannel is cheap and NOT thread-safe, so one channel is shared
    /// behind a gate rather than opened per publish.
    /// </summary>
    internal sealed class RabbitMqPublisher : IRabbitMqPublisher, IAsyncDisposable, IDisposable
    {
        private readonly RabbitMqOptions options;
        private readonly ILogger<RabbitMqPublisher> logger;
        private readonly SemaphoreSlim gate = new(1, 1);

        // Context exchanges already declared on the current channel. Cleared whenever the channel goes,
        // because a fresh channel has no memory of what a previous one declared.
        private readonly HashSet<string> declaredExchanges = new(StringComparer.Ordinal);

        private IConnection? connection;
        private IChannel? channel;
        private DateTime nextConnectAttemptUtc = DateTime.MinValue;
        private bool disposed;

        public RabbitMqPublisher(RabbitMqOptions options, ILogger<RabbitMqPublisher> logger)
        {
            this.options = options;
            this.logger = logger;
        }

        public async Task Publish(
            string routingKey, string messageId, DateTime occurredOnUtc,
            ReadOnlyMemory<byte> body, CancellationToken cancellationToken)
        {
            await gate.WaitAsync(cancellationToken);
            try
            {
                if (disposed) return;

                // The context segment of the routing key picks the exchange, so a publisher never needs
                // to know which exchange an event belongs on beyond its own contract.
                var exchange = MessagingTopology.ExchangeForRoutingKey(options.ExchangePrefix, routingKey);

                var target = await EnsureChannel(exchange, cancellationToken);
                if (target is null) return; // Broker unreachable; already logged.

                var properties = new BasicProperties
                {
                    MessageId = messageId,
                    Timestamp = new AmqpTimestamp(ToUnixSeconds(occurredOnUtc)),
                    ContentType = "application/json",
                    ContentEncoding = "utf-8",
                    Type = routingKey,
                    AppId = options.ApplicationName,
                    DeliveryMode = DeliveryModes.Persistent
                };

                await target.BasicPublishAsync(
                    exchange: exchange,
                    routingKey: routingKey,
                    mandatory: false,
                    basicProperties: properties,
                    body: body,
                    cancellationToken: cancellationToken);
            }
            finally
            {
                gate.Release();
            }
        }

        // Callers must hold the gate.
        private async Task<IChannel?> EnsureChannel(string exchange, CancellationToken cancellationToken)
        {
            if (channel is { IsOpen: true })
            {
                return await EnsureExchange(channel, exchange, cancellationToken) ? channel : null;
            }

            await CloseQuietly();

            if (DateTime.UtcNow < nextConnectAttemptUtc)
            {
                logger.LogDebug("Skipped RabbitMQ publish: connect cooldown active until {NextAttemptUtc:O}.",
                    nextConnectAttemptUtc);
                return null;
            }

            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = options.HostName,
                    Port = options.Port,
                    UserName = options.UserName,
                    Password = options.Password,
                    VirtualHost = options.VirtualHost,
                    ClientProvidedName = options.ApplicationName,
                    // Recovers a connection that was established and then dropped. It does NOT retry
                    // the first connect, which is why this method runs on every publish behind a cooldown.
                    AutomaticRecoveryEnabled = true,
                    TopologyRecoveryEnabled = true
                };

                connection = await factory.CreateConnectionAsync(cancellationToken);

                // Publisher confirms off: without an outbox there is nothing to do with a nack,
                // and tracking them only adds latency to a fire-and-forget publish.
                channel = await connection.CreateChannelAsync(
                    new CreateChannelOptions(
                        publisherConfirmationsEnabled: false,
                        publisherConfirmationTrackingEnabled: false),
                    cancellationToken);

                nextConnectAttemptUtc = DateTime.MinValue;
                logger.LogInformation(
                    "Connected to RabbitMQ at {HostName}:{Port}.", options.HostName, options.Port);

                return await EnsureExchange(channel, exchange, cancellationToken) ? channel : null;
            }
            catch (Exception exception)
            {
                nextConnectAttemptUtc = DateTime.UtcNow.AddSeconds(options.ConnectRetryCooldownSeconds);
                await CloseQuietly();
                logger.LogError(exception,
                    "Could not connect to RabbitMQ at {HostName}:{Port}. Suppressing publishes until {NextAttemptUtc:O}.",
                    options.HostName, options.Port, nextConnectAttemptUtc);
                return null;
            }
        }

        /// <summary>
        /// Declares one context exchange, once per channel lifetime.
        /// <see cref="Topology.RabbitMqTopologyInstaller"/> owns the real topology — queues, bindings,
        /// dead-letter and retry exchanges. This is only a safety net for the window in which the
        /// installer is still retrying a broker that was down at startup, where a publish would
        /// otherwise route nowhere and be silently discarded. The arguments match the installer's
        /// exactly, so this declare cannot itself be the cause of a 406.
        /// </summary>
        private async Task<bool> EnsureExchange(IChannel target, string exchange, CancellationToken cancellationToken)
        {
            if (declaredExchanges.Contains(exchange)) return true;

            try
            {
                await target.ExchangeDeclareAsync(
                    exchange: exchange,
                    type: ExchangeType.Topic,
                    durable: true,
                    autoDelete: false,
                    arguments: null,
                    cancellationToken: cancellationToken);

                declaredExchanges.Add(exchange);
                logger.LogInformation("Publishing to RabbitMQ exchange {Exchange}.", exchange);

                return true;
            }
            catch (Exception exception)
            {
                // A rejected declare closes the channel, so drop it and let the next publish reconnect.
                logger.LogError(exception,
                    "Could not declare exchange {Exchange}; dropping this publish.", exchange);
                await CloseQuietly();

                return false;
            }
        }

        private async Task CloseQuietly()
        {
            try { if (channel is not null) await channel.DisposeAsync(); }
            catch (Exception e) { logger.LogDebug(e, "Ignored error disposing the RabbitMQ channel."); }

            try { if (connection is not null) await connection.DisposeAsync(); }
            catch (Exception e) { logger.LogDebug(e, "Ignored error disposing the RabbitMQ connection."); }

            channel = null;
            connection = null;
            declaredExchanges.Clear();
        }

        private static long ToUnixSeconds(DateTime utc) =>
            new DateTimeOffset(DateTime.SpecifyKind(utc, DateTimeKind.Utc)).ToUnixTimeSeconds();

        public async ValueTask DisposeAsync()
        {
            if (disposed) return;
            await gate.WaitAsync();
            try
            {
                disposed = true;
                await CloseQuietly();
            }
            finally
            {
                gate.Release();
                gate.Dispose();
            }
        }

        // WebApplication disposes the provider asynchronously, so DisposeAsync is the path that runs.
        // This exists only so a synchronous ServiceProvider.Dispose() cannot throw.
        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            channel?.Dispose();
            connection?.Dispose();
            channel = null;
            connection = null;
            gate.Dispose();
        }
    }
}
