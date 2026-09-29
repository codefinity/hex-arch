namespace HexArch.Messaging.RabbitMQ
{
    internal interface IRabbitMqPublisher
    {
        Task Publish(
            string routingKey,
            string messageId,
            DateTime occurredOnUtc,
            ReadOnlyMemory<byte> body,
            CancellationToken cancellationToken);
    }
}
