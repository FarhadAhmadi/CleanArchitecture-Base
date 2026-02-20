using SharedKernel;

namespace Domain.Logging;

public sealed record AlertIncidentCreatedDomainEvent(Guid IncidentId) : IVersionedDomainEvent
{
    public string ContractName => "logging.alert-incident-created";
    public int ContractVersion => 1;
}
