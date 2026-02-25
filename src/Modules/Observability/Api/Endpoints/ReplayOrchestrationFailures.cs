using Application.Abstractions.Messaging;
using Application.Observability;
using Web.Api.Endpoints.Users;
using Web.Api.Extensions;

namespace Web.Api.Endpoints.Observability;

internal sealed class ReplayOrchestrationFailures : IEndpoint
{
    public sealed record Request(int Take = 100);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("observability/orchestration/replay/outbox", async (
            Request request,
            ICommandHandler<ReplayFailedOutboxCommand, IResult> handler,
            CancellationToken cancellationToken) =>
            (await handler.Handle(new ReplayFailedOutboxCommand(request.Take), cancellationToken)).Match(static x => x, Web.Api.Infrastructure.CustomResults.Problem))
        .WithTags("Observability")
        .HasPermission(Permissions.ObservabilityManage);
    }
}


