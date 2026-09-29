using HexArch.Events.IdentityAccess;
using HexArch.Events.Payment;

namespace HexArch.Messaging.RabbitMQ.Transport.Topology
{
    /// <summary>
    /// The whole broker layout, in one place.
    ///
    /// Shape: one durable topic exchange per bounded context, one quorum work queue per
    /// (consuming service x purpose), and a shared retry and dead-letter exchange pair. Publishers own
    /// exchanges and never name a queue; consumers own queues and bindings. That split is what keeps a
    /// new subscriber from being a change to the publishing side.
    ///
    /// To add a consumer, add one <c>WorkQueue</c> line. To add a context, add one <c>Context</c> line.
    /// </summary>
    public sealed record MessagingTopology(
        IReadOnlyList<ExchangeDefinition> Exchanges,
        IReadOnlyList<QueueDefinition> Queues,
        IReadOnlyList<BindingDefinition> Bindings)
    {
        /// <summary>Redeliveries a quorum queue allows before dead-lettering a poison message. Public so
        /// a consumer that republishes to the retry queue can cap its own attempt count with the same
        /// number the broker uses.</summary>
        public const int DeliveryLimit = 5;

        /// <summary>How long a message parks in the retry queue before it returns to its work queue.</summary>
        private const int RetryDelaySeconds = 30;

        /// <summary>The unprefixed name of the queue the fraud consumer drains. The consumer and the
        /// declaration both reach it through <see cref="DeactivateOnFraudQueue"/>, so they cannot drift.</summary>
        private const string DeactivateOnFraudQueueName = "identity-access.deactivate-on-fraud";

        /// <summary>The queue the fraud consumer drains, namespaced to the environment's exchange prefix
        /// exactly as the exchanges are — so a dev stack and the integration-test API never fight over
        /// one queue on a shared broker.</summary>
        public static string DeactivateOnFraudQueue(string prefix) => PrefixedQueue(prefix, DeactivateOnFraudQueueName);

        public int ResourceCount => Exchanges.Count + Queues.Count + Bindings.Count;

        public static MessagingTopology Build(RabbitMqOptions options)
        {
            var builder = new Builder(options.ExchangePrefix);

            // Bounded contexts.
            var identityAccess = builder.Context("identity-access");
            var payment = builder.Context("payment");

            // Consumers. Each of these mirrors a handler that runs in-process today, so the topology is
            // a migration target rather than a guess. Nothing drains them yet — see the note in
            // RabbitMqTopologyInstaller about applying a max-length policy before this matters.
            // An exact binding names the contract rather than restating it, so the key cannot drift out
            // of step with what the publisher puts on the wire. The wildcard bindings stay literal —
            // those are patterns, not contracts.
            builder.WorkQueue("notifications.welcome-email", identityAccess, EventContracts.For<UserRegistered>());
            builder.WorkQueue("projections.user-view-model", identityAccess, "identity-access.user.#");
            builder.WorkQueue("analytics.firehose", identityAccess, "#");

            // Deactivates a user on a fraud alert. This one is actually drained — see
            // HexArch.Listener.RabbitMQ/Consumers/FraudDetectedConsumer.cs.
            builder.WorkQueue(DeactivateOnFraudQueueName, payment, EventContracts.For<FraudDetected>());

            return builder.Build();
        }

        public static string ContextExchange(string prefix, string context) => $"{prefix}.{context}";

        /// <summary>A work queue's name, namespaced to the environment's exchange prefix like the
        /// exchanges. Two API instances with different prefixes on the same broker each own their own
        /// queues rather than colliding on a shared name (which the broker rejects as a mismatch when
        /// their arguments differ).</summary>
        public static string PrefixedQueue(string prefix, string queue) => $"{prefix}.{queue}";

        public static string DeadLetterExchange(string prefix) => $"{prefix}.dlx";

        public static string RetryExchange(string prefix) => $"{prefix}.retry";

        /// <summary>The name of a work queue's delayed-retry queue, e.g. "foo" -> "foo.retry-30s".
        /// Public so a consumer republishing a failed message uses the exact name this topology
        /// declared.</summary>
        public static string RetryQueue(string queue) => $"{queue}.retry-{RetryDelaySeconds}s";

        /// <summary>
        /// The exchange an event belongs on, from the context segment of its routing key:
        /// "identity-access.user.registered.v1" -> "hexarch.identity-access".
        /// </summary>
        public static string ExchangeForRoutingKey(string prefix, string routingKey) =>
            ContextExchange(prefix, EventContracts.ContextOf(routingKey));

        private sealed class Builder
        {
            private readonly string prefix;
            private readonly List<ExchangeDefinition> exchanges = [];
            private readonly List<QueueDefinition> queues = [];
            private readonly List<BindingDefinition> bindings = [];

            public Builder(string prefix)
            {
                this.prefix = prefix;

                exchanges.Add(new ExchangeDefinition(DeadLetterExchange(prefix)));
                exchanges.Add(new ExchangeDefinition(RetryExchange(prefix)));
            }

            /// <summary>Declares a context's exchange and returns its name for binding against.</summary>
            public string Context(string context)
            {
                var exchange = ContextExchange(prefix, context);
                exchanges.Add(new ExchangeDefinition(exchange));

                return exchange;
            }

            /// <summary>
            /// Expands one subscription into the three queues and three bindings it actually needs, so a
            /// new consumer cannot get the retry or dead-letter wiring subtly wrong. <paramref name="queue"/>
            /// is the unprefixed logical name; every declared queue is namespaced to the environment's
            /// exchange prefix, matching the exchanges.
            /// </summary>
            public void WorkQueue(string queue, string exchange, string bindingKey)
            {
                queue = PrefixedQueue(prefix, queue);
                var retryQueue = RetryQueue(queue);
                var deadQueue = $"{queue}.dead";

                queues.Add(new QueueDefinition(queue, new Dictionary<string, object?>
                {
                    ["x-queue-type"] = "quorum",
                    ["x-dead-letter-exchange"] = DeadLetterExchange(prefix),
                    ["x-dead-letter-routing-key"] = deadQueue,
                    // Poison-message guard: after this many redeliveries the broker gives up and
                    // dead-letters, with no attempt counting needed in the consumer.
                    ["x-delivery-limit"] = DeliveryLimit
                }));

                // Delayed retry for transient failures. A consumer republishes the body here via the
                // default exchange; the TTL expires and dead-letters it back to the work queue.
                // Routing that return hop through the retry exchange keyed by queue name — rather than
                // back through the context exchange — is what stops one consumer's retry from being
                // redelivered to every other queue bound to the same key.
                queues.Add(new QueueDefinition(retryQueue, new Dictionary<string, object?>
                {
                    ["x-queue-type"] = "quorum",
                    ["x-message-ttl"] = RetryDelaySeconds * 1000,
                    ["x-dead-letter-exchange"] = RetryExchange(prefix),
                    ["x-dead-letter-routing-key"] = queue
                }));

                // Terminal, so deliberately no dead-letter exchange of its own.
                queues.Add(new QueueDefinition(deadQueue, new Dictionary<string, object?>
                {
                    ["x-queue-type"] = "quorum"
                }));

                bindings.Add(new BindingDefinition(queue, exchange, bindingKey));
                bindings.Add(new BindingDefinition(queue, RetryExchange(prefix), queue));
                bindings.Add(new BindingDefinition(deadQueue, DeadLetterExchange(prefix), deadQueue));
            }

            public MessagingTopology Build() => new(exchanges, queues, bindings);
        }
    }
}
