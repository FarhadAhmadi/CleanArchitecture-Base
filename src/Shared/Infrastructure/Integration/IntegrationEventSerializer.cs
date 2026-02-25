using System.Text.Json;
using SharedKernel;

namespace Infrastructure.Integration;

public sealed class IntegrationEventSerializer : IIntegrationEventSerializer
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public OutboxMessage ToOutboxMessage(IDomainEvent domainEvent)
    {
        string clrType = domainEvent.GetType().AssemblyQualifiedName ?? domainEvent.GetType().FullName ?? domainEvent.GetType().Name;

        if (domainEvent is IVersionedDomainEvent versioned)
        {
            string envelopePayload = JsonSerializer.Serialize(
                new VersionedIntegrationEnvelope(
                    versioned.ContractName,
                    versioned.ContractVersion,
                    clrType,
                    JsonSerializer.SerializeToElement(domainEvent, domainEvent.GetType(), SerializerOptions)),
                SerializerOptions);

            return new OutboxMessage
            {
                Id = Guid.NewGuid(),
                OccurredOnUtc = DateTime.UtcNow,
                Type = versioned.ContractName,
                Payload = envelopePayload
            };
        }

        return new OutboxMessage
        {
            Id = Guid.NewGuid(),
            OccurredOnUtc = DateTime.UtcNow,
            Type = clrType,
            Payload = JsonSerializer.Serialize(domainEvent, domainEvent.GetType(), SerializerOptions)
        };
    }

    public IDomainEvent? Deserialize(string typeName, string payload)
    {
        if (TryDeserializeVersionedEnvelope(payload, out IDomainEvent? versionedEvent))
        {
            return versionedEvent;
        }

        var type = Type.GetType(typeName);
        if (type is null)
        {
            return null;
        }

        object? value = JsonSerializer.Deserialize(payload, type, SerializerOptions);
        return value as IDomainEvent;
    }

    private static bool TryDeserializeVersionedEnvelope(string payload, out IDomainEvent? domainEvent)
    {
        try
        {
            VersionedIntegrationEnvelope? envelope = JsonSerializer.Deserialize<VersionedIntegrationEnvelope>(payload, SerializerOptions);
            if (envelope is null || string.IsNullOrWhiteSpace(envelope.ClrType))
            {
                domainEvent = null;
                return false;
            }

            var type = Type.GetType(envelope.ClrType);
            if (type is null)
            {
                domainEvent = null;
                return false;
            }

            object? value = envelope.Data.Deserialize(type, SerializerOptions);
            domainEvent = value as IDomainEvent;
            return domainEvent is not null;
        }
        catch
        {
            domainEvent = null;
            return false;
        }
    }

    private sealed record VersionedIntegrationEnvelope(
        string ContractName,
        int ContractVersion,
        string ClrType,
        JsonElement Data);
}
