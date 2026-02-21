using Application.Abstractions.Messaging;
using Application.Scheduler;
using Web.Api.Endpoints.Users;
using Web.Api.Extensions;

namespace Web.Api.Endpoints.Scheduler;

internal sealed class RunJobEndpoint : IEndpoint, IOrderedEndpoint
{
    public int Order => 9;

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("scheduler/jobs/{jobId:guid}/run", async (
                Guid jobId,
                ICommandHandler<RunJobNowCommand, IResult> handler,
                CancellationToken cancellationToken) =>
            (await handler.Handle(new RunJobNowCommand(jobId), cancellationToken))
            .Match(static x => x, Web.Api.Infrastructure.CustomResults.Problem))
            .HasPermission(Permissions.SchedulerExecute)
            .WithTags(Tags.Scheduler);
    }
}

internal sealed class PauseJobEndpoint : IEndpoint, IOrderedEndpoint
{
    public int Order => 10;

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("scheduler/jobs/{jobId:guid}/pause", async (
                Guid jobId,
                ICommandHandler<PauseJobCommand, IResult> handler,
                CancellationToken cancellationToken) =>
            (await handler.Handle(new PauseJobCommand(jobId), cancellationToken))
            .Match(static x => x, Web.Api.Infrastructure.CustomResults.Problem))
            .HasPermission(Permissions.SchedulerManage)
            .WithTags(Tags.Scheduler);
    }
}

internal sealed class ResumeJobEndpoint : IEndpoint, IOrderedEndpoint
{
    public int Order => 11;

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("scheduler/jobs/{jobId:guid}/resume", async (
                Guid jobId,
                ICommandHandler<ResumeJobCommand, IResult> handler,
                CancellationToken cancellationToken) =>
            (await handler.Handle(new ResumeJobCommand(jobId), cancellationToken))
            .Match(static x => x, Web.Api.Infrastructure.CustomResults.Problem))
            .HasPermission(Permissions.SchedulerManage)
            .WithTags(Tags.Scheduler);
    }
}

internal sealed class ReplayDeadLetteredRunsEndpoint : IEndpoint, IOrderedEndpoint
{
    public int Order => 12;

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("scheduler/jobs/{jobId:guid}/replay", async (
                Guid jobId,
                ICommandHandler<ReplayDeadLetteredRunsCommand, IResult> handler,
                CancellationToken cancellationToken) =>
            (await handler.Handle(new ReplayDeadLetteredRunsCommand(jobId), cancellationToken))
            .Match(static x => x, Web.Api.Infrastructure.CustomResults.Problem))
            .HasPermission(Permissions.SchedulerManage)
            .WithTags(Tags.Scheduler);
    }
}

public sealed record QuarantineJobRequest(int? QuarantineMinutes, string? Reason);

internal sealed class QuarantineJobEndpoint : IEndpoint, IOrderedEndpoint
{
    public int Order => 13;

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("scheduler/jobs/{jobId:guid}/quarantine", async (
                Guid jobId,
                QuarantineJobRequest request,
                ICommandHandler<QuarantineJobCommand, IResult> handler,
                CancellationToken cancellationToken) =>
            (await handler.Handle(new QuarantineJobCommand(jobId, request.QuarantineMinutes, request.Reason), cancellationToken))
            .Match(static x => x, Web.Api.Infrastructure.CustomResults.Problem))
            .HasPermission(Permissions.SchedulerManage)
            .WithTags(Tags.Scheduler);
    }
}
