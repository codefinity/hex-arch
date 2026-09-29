using HexArch.Events;
using HexArch.Events.IdentityAccess;
using HexArch.Events.Payment;

namespace HexArch.Messaging.RabbitMQ.Transport.Topology
{
    /// <summary>
    /// The wire name of every domain event, in one place, next to the
    /// <see cref="MessagingTopology"/> that binds on it.
    ///
    /// Format: &lt;context&gt;.&lt;aggregate&gt;.&lt;event&gt;.&lt;version&gt; — coarse to fine, because AMQP topic
    /// wildcards only match on segment boundaries. Past tense: a routing key names a fact. Bumping the
    /// version segment is how a breaking payload change ships without breaking consumers: publish vN
    /// and vN+1 side by side for a release, then drop the old line.
    ///
    /// Mapped explicitly rather than derived from the CLR type name, so renaming a record is a refactor
    /// and not a silent change to the contract every subscriber binds on. Adding an event without an
    /// entry here fails at startup — see RabbitMqTopologyInstaller — rather than publishing into the void.
    /// </summary>
    public static class EventContracts
    {
        private static readonly IReadOnlyDictionary<Type, string> Keys = new Dictionary<Type, string>
        {
            [typeof(UserRegistered)] = "identity-access.user.registered.v1",
            [typeof(UserProfileUpdated)] = "identity-access.user.profile-updated.v1",
            [typeof(UserDeactivated)] = "identity-access.user.deactivated.v1",
            [typeof(UserReactivated)] = "identity-access.user.reactivated.v1",
            [typeof(UserRolesChanged)] = "identity-access.user.roles-changed.v1",
            [typeof(UserSessionsRevoked)] = "identity-access.user.sessions-revoked.v1",
            [typeof(UserAccountDetailsUpdated)] = "identity-access.user.account-details-updated.v1",
            [typeof(UserPasswordChanged)] = "identity-access.user.password-changed.v1",
            [typeof(UserEmailVerified)] = "identity-access.user.email-verified.v1",
            [typeof(UserLockedOut)] = "identity-access.user.locked-out.v1",
            [typeof(UserAccountClosed)] = "identity-access.user.account-closed.v1",
            [typeof(FraudDetected)] = "payment.fraud.detected.v1",
            [typeof(FraudCleared)] = "payment.fraud.cleared.v1"
        };

        public static string For<TEvent>() where TEvent : IDomainEvent => For(typeof(TEvent));

        public static string For(Type eventType)
        {
            if (!Keys.TryGetValue(eventType, out var routingKey))
            {
                throw new InvalidOperationException(
                    $"{eventType.FullName} has no entry in {nameof(EventContracts)}. Every domain event " +
                    "needs an explicit routing key of the form <context>.<aggregate>.<event>.<version>.");
            }

            return routingKey;
        }

        /// <summary>
        /// The leading segment of a routing key, which names the bounded context that owns the event and
        /// therefore the exchange it publishes to.
        /// </summary>
        public static string ContextOf(string routingKey)
        {
            var separator = routingKey.IndexOf('.');

            return separator > 0 ? routingKey[..separator] : routingKey;
        }

        /// <summary>
        /// Domain events the contracts assembly defines that are missing from <see cref="Keys"/>. Nothing
        /// forces an entry at compile time, so startup checks this instead.
        /// </summary>
        public static IReadOnlyList<Type> Unmapped() =>
            typeof(IDomainEvent).Assembly
                .GetTypes()
                .Where(type => type is { IsClass: true, IsAbstract: false } && typeof(IDomainEvent).IsAssignableFrom(type))
                .Where(type => !Keys.ContainsKey(type))
                .OrderBy(type => type.FullName, StringComparer.Ordinal)
                .ToList();
    }
}
