using Infrastructure.Monitoring;
using Web.Api.Endpoints.Users;
using Web.Api.Extensions;

namespace Web.Api.Endpoints.Observability;

internal sealed class GetEventCatalog : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("observability/events/catalog", (
            EventContractCatalogService catalogService) =>
        {
            IReadOnlyList<EventContractDescriptor> contracts = catalogService.GetContracts();
            return Results.Ok(new
            {
                count = contracts.Count,
                items = contracts
            });
        })
        .WithTags("Observability")
        .HasPermission(Permissions.ObservabilityRead);
    }
}
