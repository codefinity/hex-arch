namespace HexArch.Messaging.RabbitMQ.Transport
{
    public record RabbitMqOptions(
        string HostName,
        // Exchanges are named "<prefix>.<bounded-context>", plus "<prefix>.dlx" and "<prefix>.retry";
        // work queues are "<prefix>.<queue>". Environments that share a broker get their own prefix
        // rather than their own vhost, so every exchange and queue is isolated by it.
        string ExchangePrefix,
        int Port = 5672,
        string UserName = "guest",
        string Password = "guest",
        string VirtualHost = "/",
        string ApplicationName = "hexarch-api",
        int PublishTimeoutSeconds = 5,
        int ConnectRetryCooldownSeconds = 30,
        bool DeclareTopologyOnStartup = true);
}
