namespace Application.Abstractions.Observability;

public interface IEventContractCatalogService
{
    IReadOnlyList<EventContractDescriptor> GetContracts();
}

public sealed record EventContractDescriptor(
    string ContractName,
    int ContractVersion,
    string ClrType,
    string AssemblyName);
