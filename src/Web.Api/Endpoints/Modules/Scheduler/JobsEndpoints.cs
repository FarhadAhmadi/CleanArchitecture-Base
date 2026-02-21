using Application.Abstractions.Messaging;
using Application.Scheduler;
using Domain.Scheduler;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Endpoints.Common.Requests;
using Web.Api.Endpoints.Users;
using Web.Api.Extensions;

namespace Web.Api.Endpoints.Scheduler;

public sealed record CreateJobRequest(
    string Name,
    string? Description,
    JobType? Type,
    string? PayloadJson,
    int? MaxRetryAttempts,
    int? RetryBackoffSeconds,
    int? MaxExecutionSeconds,
    int? MaxConsecutiveFailures);

public sealed record UpdateJobRequest(
    string Name,
    string? Description,
    JobType? Type,
    string? PayloadJson,
    int? MaxRetryAttempts,
    int? RetryBackoffSeconds,
    int? MaxExecutionSeconds,
    int? MaxConsecutiveFailures);

public sealed class ListJobsRequest : PagedSortedSearchQueryRequest
{
    public JobStatus? Status { get; set; }
}

internal sealed class CreateJobEndpoint : IEndpoint, IOrderedEndpoint
{
    public int Order => 1;

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("scheduler/jobs", async (
                CreateJobRequest request,
                ICommandHandler<CreateJobCommand, IResult> handler,
                CancellationToken cancellationToken) =>
            (await handler.Handle(
                new CreateJobCommand(
                    request.Name,
                    request.Description,
                    request.Type,
                    request.PayloadJson,
                    request.MaxRetryAttempts,
                    request.RetryBackoffSeconds,
                    request.MaxExecutionSeconds,
                    request.MaxConsecutiveFailures),
                cancellationToken)).Match(static x => x, Web.Api.Infrastructure.CustomResults.Problem))
            .HasPermission(Permissions.SchedulerWrite)
            .WithTags(Tags.Scheduler);
    }
}

internal sealed class ListJobsEndpoint : IEndpoint, IOrderedEndpoint
{
    public int Order => 2;

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("scheduler/jobs", async (
                [AsParameters] ListJobsRequest request,
                IQueryHandler<ListJobsQuery, IResult> handler,
                CancellationToken cancellationToken) =>
            (await handler.Handle(
                new ListJobsQuery(request.Page, request.PageIndex, request.PageSize, request.Status, request.Search),
                cancellationToken)).Match(static x => x, Web.Api.Infrastructure.CustomResults.Problem))
            .HasPermission(Permissions.SchedulerRead)
            .WithTags(Tags.Scheduler);
    }
}

internal sealed class GetJobByIdEndpoint : IEndpoint, IOrderedEndpoint
{
    public int Order => 3;

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("scheduler/jobs/{jobId:guid}", async (
                Guid jobId,
                IQueryHandler<GetJobByIdQuery, IResult> handler,
                CancellationToken cancellationToken) =>
            (await handler.Handle(new GetJobByIdQuery(jobId), cancellationToken))
            .Match(static x => x, Web.Api.Infrastructure.CustomResults.Problem))
            .HasPermission(Permissions.SchedulerRead)
            .WithTags(Tags.Scheduler);
    }
}

internal sealed class UpdateJobEndpoint : IEndpoint, IOrderedEndpoint
{
    public int Order => 4;

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("scheduler/jobs/{jobId:guid}", async (
                Guid jobId,
                UpdateJobRequest request,
                ICommandHandler<UpdateJobCommand, IResult> handler,
                CancellationToken cancellationToken) =>
            (await handler.Handle(
                new UpdateJobCommand(
                    jobId,
                    request.Name,
                    request.Description,
                    request.Type,
                    request.PayloadJson,
                    request.MaxRetryAttempts,
                    request.RetryBackoffSeconds,
                    request.MaxExecutionSeconds,
                    request.MaxConsecutiveFailures),
                cancellationToken)).Match(static x => x, Web.Api.Infrastructure.CustomResults.Problem))
            .HasPermission(Permissions.SchedulerWrite)
            .WithTags(Tags.Scheduler);
    }
}

internal sealed class DisableJobEndpoint : IEndpoint, IOrderedEndpoint
{
    public int Order => 5;

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("scheduler/jobs/{jobId:guid}", async (
                Guid jobId,
                ICommandHandler<DisableJobCommand, IResult> handler,
                CancellationToken cancellationToken) =>
            (await handler.Handle(new DisableJobCommand(jobId), cancellationToken))
            .Match(static x => x, Web.Api.Infrastructure.CustomResults.Problem))
            .HasPermission(Permissions.SchedulerManage)
            .WithTags(Tags.Scheduler);
    }
}
