using Application.Abstractions.Messaging;
using Application.Scheduler;
using Web.Api.Endpoints.Users;
using Web.Api.Extensions;

namespace Web.Api.Endpoints.Scheduler;

public sealed record AddJobDependencyRequest(Guid DependsOnJobId);

internal sealed class AddJobDependencyEndpoint : IEndpoint, IOrderedEndpoint
{
    public int Order => 17;

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("scheduler/jobs/{jobId:guid}/dependencies", async (
                Guid jobId,
                AddJobDependencyRequest request,
                ICommandHandler<AddJobDependencyCommand, IResult> handler,
                CancellationToken cancellationToken) =>
            (await handler.Handle(
                new AddJobDependencyCommand(jobId, request.DependsOnJobId),
                cancellationToken)).Match(static x => x, Web.Api.Infrastructure.CustomResults.Problem))
            .HasPermission(Permissions.SchedulerManage)
            .WithTags(Tags.Scheduler);
    }
}

internal sealed class GetJobDependenciesEndpoint : IEndpoint, IOrderedEndpoint
{
    public int Order => 18;

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("scheduler/jobs/{jobId:guid}/dependencies", async (
                Guid jobId,
                IQueryHandler<GetJobDependenciesQuery, IResult> handler,
                CancellationToken cancellationToken) =>
            (await handler.Handle(new GetJobDependenciesQuery(jobId), cancellationToken))
            .Match(static x => x, Web.Api.Infrastructure.CustomResults.Problem))
            .HasPermission(Permissions.SchedulerRead)
            .WithTags(Tags.Scheduler);
    }
}

internal sealed class RemoveJobDependencyEndpoint : IEndpoint, IOrderedEndpoint
{
    public int Order => 19;

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("scheduler/jobs/{jobId:guid}/dependencies/{dependsOnJobId:guid}", async (
                Guid jobId,
                Guid dependsOnJobId,
                ICommandHandler<RemoveJobDependencyCommand, IResult> handler,
                CancellationToken cancellationToken) =>
            (await handler.Handle(new RemoveJobDependencyCommand(jobId, dependsOnJobId), cancellationToken))
            .Match(static x => x, Web.Api.Infrastructure.CustomResults.Problem))
            .HasPermission(Permissions.SchedulerManage)
            .WithTags(Tags.Scheduler);
    }
}

