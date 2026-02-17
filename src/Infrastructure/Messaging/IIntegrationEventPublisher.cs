namespace Infrastructure.Messaging;

internal interface IIntegrationEventPublisher
{
    Task PublishAsync(Guid messageId, string messageType, string payload, CancellationToken cancellationToken);
}
