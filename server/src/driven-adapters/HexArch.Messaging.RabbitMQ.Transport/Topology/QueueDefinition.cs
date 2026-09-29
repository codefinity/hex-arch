namespace HexArch.Messaging.RabbitMQ.Transport.Topology
{
    /// <summary>
    /// One queue to declare. <see cref="Arguments"/> are immutable once the queue exists — changing any
    /// of them later requires deleting and recreating the queue — so anything expected to be tuned in
    /// production (max length, TTL on a work queue) belongs in a broker policy instead.
    /// </summary>
    public sealed record QueueDefinition(
        string Name,
        IDictionary<string, object?> Arguments,
        bool Durable = true);
}
