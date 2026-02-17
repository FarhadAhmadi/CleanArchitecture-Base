using SharedKernel;

namespace Infrastructure.Integration;

internal interface IIntegrationEventSerializer
{
    OutboxMessage ToOutboxMessage(IDomainEvent domainEvent);
    IDomainEvent? Deserialize(string typeName, string payload);
}
