namespace HexArch.Messaging.RabbitMQ.Transport.Topology
{
    /// <summary>
    /// One exchange to declare. Topic throughout: direct and fanout are both special cases of it, and
    /// starting anywhere else forecloses wildcard subscriptions that cost nothing to keep open.
    /// </summary>
    public sealed record ExchangeDefinition(string Name, string Type = "topic", bool Durable = true);
}
