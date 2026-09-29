using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace HexArch.Messaging.RabbitMQ.Transport.Topology
{
    /// <summary>
    /// Brings the broker to the shape described by <see cref="MessagingTopology"/> at startup, creating
    /// anything that does not already exist. Every declare is idempotent, so this runs on every boot.
    ///
    /// Deliberately does not block startup: it runs on its own short-lived connection (the publisher's
    /// channel is private and gated) and a broker outage degrades messaging rather than taking the API
    /// down with it, matching the at-most-once stance the publisher already takes.
    /// </summary>
    internal sealed class RabbitMqTopologyInstaller : BackgroundService
    {
        private readonly RabbitMqOptions options;
        private readonly ILogger<RabbitMqTopologyInstaller> logger;

        public RabbitMqTopologyInstaller(RabbitMqOptions options, ILogger<RabbitMqTopologyInstaller> logger)
        {
            this.options = options;
            this.logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Before the first await, so a missing EventContracts entry fails the host at startup rather
            // than surfacing as a type initializer failure on the first publish of that event. Nothing
            // in the compiler requires an entry, so this check is what keeps the map honest.
            VerifyEveryEventHasARoutingKey();

            if (!options.DeclareTopologyOnStartup)
            {
                logger.LogInformation("Skipped RabbitMQ topology declaration: DeclareTopologyOnStartup is false.");
                return;
            }

            var topology = MessagingTopology.Build(options);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await DeclareAll(topology, stoppingToken);

                    logger.LogInformation(
                        "Declared RabbitMQ topology on {HostName}:{Port}: {ExchangeCount} exchange(s), " +
                        "{QueueCount} queue(s), {BindingCount} binding(s).",
                        options.HostName, options.Port,
                        topology.Exchanges.Count, topology.Queues.Count, topology.Bindings.Count);

                    return;
                }
                catch (TopologyMismatchException exception)
                {
                    // 406: what is already on the broker disagrees with what we asked for. Retrying
                    // cannot fix that, so stop and let an operator delete the offending resource.
                    logger.LogError(
                        "RabbitMQ topology mismatch on {Resource}: {Reason}. Delete it on the broker and " +
                        "restart; leaving the remaining topology undeclared.",
                        exception.Resource, exception.Reason);

                    return;
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    return;
                }
                catch (Exception exception)
                {
                    logger.LogWarning(exception,
                        "Could not declare the RabbitMQ topology on {HostName}:{Port}. Retrying in {Cooldown}s.",
                        options.HostName, options.Port, options.ConnectRetryCooldownSeconds);
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

        private static void VerifyEveryEventHasARoutingKey()
        {
            var unmapped = EventContracts.Unmapped();

            if (unmapped.Count > 0)
            {
                throw new InvalidOperationException(
                    $"These domain events have no entry in {nameof(EventContracts)} and would be " +
                    "unroutable: " + string.Join(", ", unmapped.Select(type => type.FullName)) + ".");
            }
        }

        private async Task DeclareAll(MessagingTopology topology, CancellationToken cancellationToken)
        {
            var factory = new ConnectionFactory
            {
                HostName = options.HostName,
                Port = options.Port,
                UserName = options.UserName,
                Password = options.Password,
                VirtualHost = options.VirtualHost,
                ClientProvidedName = $"{options.ApplicationName}-topology"
            };

            IConnection? connection = null;
            IChannel? channel = null;

            try
            {
                connection = await factory.CreateConnectionAsync(cancellationToken);
                channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

                // Non-nullable alias, so the closures below do not each need a null check.
                var target = channel;

                foreach (var exchange in topology.Exchanges)
                {
                    await Declaring($"exchange '{exchange.Name}'", () => target.ExchangeDeclareAsync(
                        exchange: exchange.Name,
                        type: exchange.Type,
                        durable: exchange.Durable,
                        autoDelete: false,
                        arguments: null,
                        cancellationToken: cancellationToken));
                }

                foreach (var queue in topology.Queues)
                {
                    await Declaring($"queue '{queue.Name}'", () => target.QueueDeclareAsync(
                        queue: queue.Name,
                        durable: queue.Durable,
                        exclusive: false,
                        autoDelete: false,
                        arguments: queue.Arguments,
                        cancellationToken: cancellationToken));
                }

                foreach (var binding in topology.Bindings)
                {
                    await Declaring(
                        $"binding '{binding.Queue}' -> '{binding.Exchange}' ({binding.RoutingKey})",
                        () => target.QueueBindAsync(
                            queue: binding.Queue,
                            exchange: binding.Exchange,
                            routingKey: binding.RoutingKey,
                            arguments: null,
                            cancellationToken: cancellationToken));
                }
            }
            finally
            {
                await CloseQuietly(channel, connection);
            }
        }

        /// <summary>
        /// Runs one declare, translating a 406 into a failure that names the resource. The broker closes
        /// the channel on a 406, so the first mismatch ends the pass either way.
        /// </summary>
        private static async Task Declaring(string resource, Func<Task> declare)
        {
            try
            {
                await declare();
            }
            catch (OperationInterruptedException exception)
                when (exception.ShutdownReason?.ReplyCode == Constants.PreconditionFailed)
            {
                throw new TopologyMismatchException(resource, exception.ShutdownReason?.ReplyText ?? "unknown");
            }
        }

        private async Task CloseQuietly(IChannel? channel, IConnection? connection)
        {
            try { if (channel is not null) await channel.DisposeAsync(); }
            catch (Exception e) { logger.LogDebug(e, "Ignored error disposing the topology channel."); }

            try { if (connection is not null) await connection.DisposeAsync(); }
            catch (Exception e) { logger.LogDebug(e, "Ignored error disposing the topology connection."); }
        }

        private sealed class TopologyMismatchException : Exception
        {
            public TopologyMismatchException(string resource, string reason)
                : base($"{resource}: {reason}")
            {
                Resource = resource;
                Reason = reason;
            }

            public string Resource { get; }

            public string Reason { get; }
        }
    }
}
