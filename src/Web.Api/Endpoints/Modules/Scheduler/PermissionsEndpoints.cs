using Application.Abstractions.Messaging;
using Application.Scheduler;
using Web.Api.Endpoints.Users;
using Web.Api.Extensions;

namespace Web.Api.Endpoints.Scheduler;

public sealed record UpsertJobPermissionRequest(string SubjectType, string SubjectValue, bool CanRead, bool CanManage);

internal sealed class GetJobPermissionsEndpoint : IEndpoint, IOrderedEndpoint
{
    public int Order => 15;

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("scheduler/jobs/{jobId:guid}/permissions", async (
                Guid jobId,
                IQueryHandler<GetJobPermissionsQuery, IResult> handler,
                CancellationToken cancellationToken) =>
            (await handler.Handle(new GetJobPermissionsQuery(jobId), cancellationToken))
            .Match(static x => x, Web.Api.Infrastructure.CustomResults.Problem))
            .HasPermission(Permissions.SchedulerPermissionsManage)
            .WithTags(Tags.Scheduler);
    }
}

internal sealed class UpsertJobPermissionsEndpoint : IEndpoint, IOrderedEndpoint
{
    public int Order => 16;

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("scheduler/jobs/{jobId:guid}/permissions", async (
                Guid jobId,
                UpsertJobPermissionRequest request,
                ICommandHandler<UpsertJobPermissionsCommand, IResult> handler,
                CancellationToken cancellationToken) =>
            (await handler.Handle(
                new UpsertJobPermissionsCommand(
                    jobId,
                    request.SubjectType,
                    request.SubjectValue,
                    request.CanRead,
                    request.CanManage),
                cancellationToken)).Match(static x => x, Web.Api.Infrastructure.CustomResults.Problem))
            .HasPermission(Permissions.SchedulerPermissionsManage)
            .WithTags(Tags.Scheduler);
    }
}

