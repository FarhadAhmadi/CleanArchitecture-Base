namespace SharedKernel;

public interface IVersionedDomainEvent : IDomainEvent
{
    string ContractName { get; }
    int ContractVersion { get; }
}
