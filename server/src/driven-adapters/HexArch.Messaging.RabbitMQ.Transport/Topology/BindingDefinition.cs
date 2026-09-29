namespace HexArch.Messaging.RabbitMQ.Transport.Topology
{
    /// <summary>
    /// One queue-to-exchange binding. Bindings are owned by the consuming side: a publisher never names
    /// a queue, so adding a subscriber is a change here and never a change to the publisher.
    /// </summary>
    public sealed record BindingDefinition(string Queue, string Exchange, string RoutingKey);
}
