using System.Reflection;
using System.Text.RegularExpressions;
using Application.Abstractions.Observability;
using SharedKernel;

namespace Infrastructure.Monitoring;

public sealed class EventContractCatalogService : IEventContractCatalogService
{
    public IReadOnlyList<EventContractDescriptor> GetContracts()
    {
        Assembly[] assemblies =
        [
            typeof(Domain.Users.User).Assembly,
            typeof(Application.DependencyInjection).Assembly
        ];

        List<EventContractDescriptor> contracts = [];

        foreach (Assembly assembly in assemblies.Distinct())
        {
            IEnumerable<Type> eventTypes = assembly
                .GetTypes()
                .Where(t => t is { IsAbstract: false, IsInterface: false } && typeof(IDomainEvent).IsAssignableFrom(t));

            foreach (Type eventType in eventTypes)
            {
                contracts.Add(CreateDescriptor(eventType));
            }
        }

        return contracts
            .OrderBy(x => x.ContractName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(x => x.ContractVersion)
            .ToArray();
    }

    private static EventContractDescriptor CreateDescriptor(Type eventType)
    {
        string contractName = eventType.FullName ?? eventType.Name;
        int contractVersion = 1;

        if (typeof(IVersionedDomainEvent).IsAssignableFrom(eventType))
        {
            contractName = BuildContractNameFromType(eventType);
            contractVersion = 1;
        }

        return new EventContractDescriptor(
            contractName,
            contractVersion,
            eventType.FullName ?? eventType.Name,
            eventType.Assembly.GetName().Name ?? "unknown");
    }

    private static string BuildContractNameFromType(Type eventType)
    {
        string raw = eventType.Name.EndsWith("DomainEvent", StringComparison.Ordinal)
            ? eventType.Name[..^"DomainEvent".Length]
            : eventType.Name;
        return Regex.Replace(raw, "(?<!^)([A-Z])", ".$1", RegexOptions.CultureInvariant);
    }
}
