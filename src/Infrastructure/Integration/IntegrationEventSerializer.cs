using System.Text.Json;
using SharedKernel;

namespace Infrastructure.Integration;

public sealed class IntegrationEventSerializer : IIntegrationEventSerializer
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public OutboxMessage ToOutboxMessage(IDomainEvent domainEvent)
    {
        return new OutboxMessage
        {
            Id = Guid.NewGuid(),
            OccurredOnUtc = DateTime.UtcNow,
            Type = domainEvent.GetType().AssemblyQualifiedName ?? domainEvent.GetType().FullName ?? domainEvent.GetType().Name,
            Payload = JsonSerializer.Serialize(domainEvent, domainEvent.GetType(), SerializerOptions)
        };
    }

    public IDomainEvent? Deserialize(string typeName, string payload)
    {
        var type = Type.GetType(typeName);
        if (type is null)
        {
            return null;
        }

        object? value = JsonSerializer.Deserialize(payload, type, SerializerOptions);
        return value as IDomainEvent;
    }
}
