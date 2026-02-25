using Application.Abstractions.Messaging;
using Application.Observability;
using Web.Api.Endpoints.Users;
using Web.Api.Extensions;

namespace Web.Api.Endpoints.Observability;

internal sealed class GetOrchestrationHealth : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("dashboard/orchestration-health", async (
            IQueryHandler<GetOrchestrationHealthQuery, IResult> handler,
            CancellationToken cancellationToken) =>
            (await handler.Handle(new GetOrchestrationHealthQuery(), cancellationToken)).Match(static x => x, Web.Api.Infrastructure.CustomResults.Problem))
        .WithTags("Observability")
        .HasPermission(Permissions.ObservabilityRead);
    }
}


