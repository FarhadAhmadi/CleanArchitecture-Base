namespace Infrastructure.Messaging;

public sealed class RabbitMqOptions
{
    public const string SectionName = "RabbitMq";

    public bool Enabled { get; init; }
    public string Host { get; init; } = "localhost";
    public int Port { get; init; } = 5672;
    public string UserName { get; init; } = "guest";
    public string Password { get; init; } = "guest";
    public string VirtualHost { get; init; } = "/";
    public string ExchangeName { get; init; } = "cleanarchitecture.events";
    public string QueueName { get; init; } = "cleanarchitecture.events.queue";
    public string RoutingKey { get; init; } = "#";
    public bool ConsumerEnabled { get; init; } = true;
}
