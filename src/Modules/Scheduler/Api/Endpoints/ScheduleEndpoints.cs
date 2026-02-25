using Application.Abstractions.Messaging;
using Application.Scheduler;
using Domain.Scheduler;
using Web.Api.Endpoints.Users;
using Web.Api.Extensions;

namespace Web.Api.Endpoints.Scheduler;

public sealed record UpsertJobScheduleRequest(
    ScheduleType Type,
    string? CronExpression,
    int? IntervalSeconds,
    DateTime? OneTimeAtUtc,
    DateTime? StartAtUtc,
    DateTime? EndAtUtc,
    MisfirePolicy MisfirePolicy,
    int? MaxCatchUpRuns);

internal sealed class UpsertJobScheduleEndpoint : IEndpoint, IOrderedEndpoint
{
    public int Order => 6;

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("scheduler/jobs/{jobId:guid}/schedule", async (
                Guid jobId,
                UpsertJobScheduleRequest request,
                ICommandHandler<UpsertJobScheduleCommand, IResult> handler,
                CancellationToken cancellationToken) =>
            (await handler.Handle(
                new UpsertJobScheduleCommand(
                    jobId,
                    request.Type,
                    request.CronExpression,
                    request.IntervalSeconds,
                    request.OneTimeAtUtc,
                    request.StartAtUtc,
                    request.EndAtUtc,
                    request.MisfirePolicy,
                    request.MaxCatchUpRuns),
                cancellationToken)).Match(static x => x, Web.Api.Infrastructure.CustomResults.Problem))
            .HasPermission(Permissions.SchedulerWrite)
            .WithTags(Tags.Scheduler);
    }
}

internal sealed class GetJobScheduleEndpoint : IEndpoint, IOrderedEndpoint
{
    public int Order => 7;

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("scheduler/jobs/{jobId:guid}/schedule", async (
                Guid jobId,
                IQueryHandler<GetJobScheduleQuery, IResult> handler,
                CancellationToken cancellationToken) =>
            (await handler.Handle(new GetJobScheduleQuery(jobId), cancellationToken))
            .Match(static x => x, Web.Api.Infrastructure.CustomResults.Problem))
            .HasPermission(Permissions.SchedulerRead)
            .WithTags(Tags.Scheduler);
    }
}

internal sealed class DeleteJobScheduleEndpoint : IEndpoint, IOrderedEndpoint
{
    public int Order => 8;

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("scheduler/jobs/{jobId:guid}/schedule", async (
                Guid jobId,
                ICommandHandler<DeleteJobScheduleCommand, IResult> handler,
                CancellationToken cancellationToken) =>
            (await handler.Handle(new DeleteJobScheduleCommand(jobId), cancellationToken))
            .Match(static x => x, Web.Api.Infrastructure.CustomResults.Problem))
            .HasPermission(Permissions.SchedulerManage)
            .WithTags(Tags.Scheduler);
    }
}
