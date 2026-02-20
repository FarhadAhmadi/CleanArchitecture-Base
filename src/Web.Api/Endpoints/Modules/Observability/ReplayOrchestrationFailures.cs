using Infrastructure.Monitoring;
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
            OrchestrationReplayService replayService,
            CancellationToken cancellationToken) =>
        {
            int replayed = await replayService.ReplayFailedOutboxAsync(request.Take, cancellationToken);
            return Results.Ok(new { target = "outbox", replayed });
        })
        .WithTags("Observability")
        .HasPermission(Permissions.ObservabilityManage);

        app.MapPost("observability/orchestration/replay/inbox", async (
            Request request,
            OrchestrationReplayService replayService,
            CancellationToken cancellationToken) =>
        {
            int replayed = await replayService.ReplayFailedInboxAsync(request.Take, cancellationToken);
            return Results.Ok(new { target = "inbox", replayed });
        })
        .WithTags("Observability")
        .HasPermission(Permissions.ObservabilityManage);
    }
}
