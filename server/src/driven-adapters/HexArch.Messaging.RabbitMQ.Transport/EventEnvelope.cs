namespace HexArch.Messaging.RabbitMQ.Transport
{
    public sealed record EventEnvelope<TEvent>(
        string EventId,
        string Type,
        DateTime OccurredOnUtc,
        int Version,
        string Source,
        TEvent Data);
}
