using System.Text;
using Infrastructure.DomainEvents;
using Infrastructure.Integration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using SharedKernel;

namespace Infrastructure.Messaging;

internal sealed class RabbitMqInboxWorker(
    RabbitMqOptions options,
    IServiceScopeFactory scopeFactory,
    ILogger<RabbitMqInboxWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!options.Enabled || !options.ConsumerEnabled)
        {
            return;
        }

        ConnectionFactory factory = new()
        {
            HostName = options.Host,
            Port = options.Port,
            UserName = options.UserName,
            Password = options.Password,
            VirtualHost = options.VirtualHost
        };

        using IConnection connection = factory.CreateConnection();
        using IModel channel = connection.CreateModel();

        channel.ExchangeDeclare(options.ExchangeName, ExchangeType.Topic, durable: true, autoDelete: false);
        channel.QueueDeclare(options.QueueName, durable: true, exclusive: false, autoDelete: false);
        channel.QueueBind(options.QueueName, options.ExchangeName, options.RoutingKey);
        channel.BasicQos(prefetchSize: 0, prefetchCount: 20, global: false);

        while (!stoppingToken.IsCancellationRequested)
        {
            BasicGetResult? result = channel.BasicGet(options.QueueName, autoAck: false);
            if (result is null)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(500), stoppingToken);
                continue;
            }

            string messageId = result.BasicProperties.MessageId ?? Guid.NewGuid().ToString("N");
            string messageType = result.BasicProperties.Type ?? "unknown";
            string payload = Encoding.UTF8.GetString(result.Body.Span);

            using IServiceScope scope = scopeFactory.CreateScope();
            IInboxStore inboxStore = scope.ServiceProvider.GetRequiredService<IInboxStore>();
            IIntegrationEventSerializer serializer = scope.ServiceProvider.GetRequiredService<IIntegrationEventSerializer>();
            IDomainEventsDispatcher dispatcher = scope.ServiceProvider.GetRequiredService<IDomainEventsDispatcher>();

            try
            {
                bool started = await inboxStore.TryStartAsync(messageId, messageType, payload, stoppingToken);
                if (!started)
                {
                    channel.BasicAck(result.DeliveryTag, multiple: false);
                    continue;
                }

                if (serializer.Deserialize(messageType, payload) is not IDomainEvent domainEvent)
                {
                    throw new InvalidOperationException($"Cannot deserialize event with type '{messageType}'.");
                }

                await dispatcher.DispatchAsync([domainEvent], stoppingToken);
                await inboxStore.MarkProcessedAsync(messageId, stoppingToken);
                channel.BasicAck(result.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                await inboxStore.MarkFailedAsync(messageId, ex.Message, stoppingToken);
                channel.BasicAck(result.DeliveryTag, multiple: false);
                logger.LogError(ex, "Inbox processing failed. MessageId={MessageId} Type={Type}", messageId, messageType);
            }
        }
    }
}
