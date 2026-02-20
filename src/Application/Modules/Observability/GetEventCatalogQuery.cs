using Application.Abstractions.Messaging;
using Application.Abstractions.Observability;

namespace Application.Observability;

public sealed record GetEventCatalogQuery() : IQuery<IResult>;
internal sealed class GetEventCatalogQueryHandler(IEventContractCatalogService catalogService) : ResultWrappingQueryHandler<GetEventCatalogQuery>
{
    protected override async Task<IResult> HandleCore(GetEventCatalogQuery query, CancellationToken cancellationToken)
    {
        _ = query;
        _ = cancellationToken;
        IReadOnlyList<EventContractDescriptor> contracts = catalogService.GetContracts();
        return Results.Ok(new { count = contracts.Count, items = contracts });
    }
}




