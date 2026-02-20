using System.Text;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Infrastructure.Messaging;

internal sealed class RabbitMqIntegrationEventPublisher(
    RabbitMqOptions options,
    ILogger<RabbitMqIntegrationEventPublisher> logger) : IIntegrationEventPublisher, IDisposable
{
    private readonly object _sync = new();
    private IConnection? _connection;
    private IModel? _channel;

    public Task PublishAsync(Guid messageId, string messageType, string payload, CancellationToken cancellationToken)
    {
        if (!options.Enabled)
        {
            return Task.CompletedTask;
        }

        lock (_sync)
        {
            EnsureConnected();

            IBasicProperties properties = _channel!.CreateBasicProperties();
            properties.Persistent = true;
            properties.MessageId = messageId.ToString("N");
            properties.Type = messageType;
            properties.ContentType = "application/json";
            properties.DeliveryMode = 2;
            properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            properties.CorrelationId = Activity.Current?.TraceId.ToString() ?? Guid.NewGuid().ToString("N");

            byte[] body = Encoding.UTF8.GetBytes(payload);
            _channel.BasicPublish(
                exchange: options.ExchangeName,
                routingKey: messageType,
                basicProperties: properties,
                body: body);

            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(
                    "Published integration event. MessageId={MessageId} Type={Type} Exchange={Exchange}",
                    messageId,
                    messageType,
                    options.ExchangeName);
            }
        }

        return Task.CompletedTask;
    }

    private void EnsureConnected()
    {
        if (_connection is { IsOpen: true } && _channel is { IsOpen: true })
        {
            return;
        }

        ConnectionFactory factory = new()
        {
            HostName = options.Host,
            Port = options.Port,
            UserName = options.UserName,
            Password = options.Password,
            VirtualHost = options.VirtualHost,
            DispatchConsumersAsync = false
        };

        _connection?.Dispose();
        _channel?.Dispose();

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        _channel.ExchangeDeclare(options.ExchangeName, ExchangeType.Topic, durable: true, autoDelete: false);
        _channel.QueueDeclare(options.QueueName, durable: true, exclusive: false, autoDelete: false);
        _channel.QueueBind(options.QueueName, options.ExchangeName, options.RoutingKey);
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}
