using Application.Abstractions.Messaging;
using Application.Observability;
using Web.Api.Endpoints.Users;
using Web.Api.Extensions;

namespace Web.Api.Endpoints.Observability;

internal sealed class GetEventCatalog : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("observability/events/catalog", async (
            IQueryHandler<GetEventCatalogQuery, IResult> handler,
            CancellationToken cancellationToken) =>
            (await handler.Handle(new GetEventCatalogQuery(), cancellationToken)).Match(static x => x, Web.Api.Infrastructure.CustomResults.Problem))
        .WithTags("Observability")
        .HasPermission(Permissions.ObservabilityRead);
    }
}


