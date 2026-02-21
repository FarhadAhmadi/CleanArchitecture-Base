using Application.Abstractions.Messaging;
using Application.Scheduler;
using Domain.Scheduler;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Endpoints.Users;
using Web.Api.Extensions;

namespace Web.Api.Endpoints.Scheduler;

public sealed class ListSchedulerLogsRequest
{
    public int? Page { get; set; }
    public int? PageIndex { get; set; }
    public int? PageSize { get; set; }
    public Guid? JobId { get; set; }
    public JobExecutionStatus? Status { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
}

internal sealed class GetJobExecutionLogsEndpoint : IEndpoint, IOrderedEndpoint
{
    public int Order => 12;

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("scheduler/jobs/{jobId:guid}/logs", async (
                Guid jobId,
                [AsParameters] ListSchedulerLogsRequest request,
                IQueryHandler<GetJobExecutionLogsQuery, IResult> handler,
                CancellationToken cancellationToken) =>
            (await handler.Handle(
                new GetJobExecutionLogsQuery(jobId, request.Page, request.PageIndex, request.PageSize),
                cancellationToken)).Match(static x => x, Web.Api.Infrastructure.CustomResults.Problem))
            .HasPermission(Permissions.SchedulerRead)
            .WithTags(Tags.Scheduler);
    }
}

internal sealed class GetAllJobExecutionLogsEndpoint : IEndpoint, IOrderedEndpoint
{
    public int Order => 13;

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("scheduler/jobs/logs", async (
                [AsParameters] ListSchedulerLogsRequest request,
                IQueryHandler<GetAllJobExecutionLogsQuery, IResult> handler,
                CancellationToken cancellationToken) =>
            (await handler.Handle(
                new GetAllJobExecutionLogsQuery(
                    request.Page,
                    request.PageIndex,
                    request.PageSize,
                    request.JobId,
                    request.Status,
                    request.From,
                    request.To),
                cancellationToken)).Match(static x => x, Web.Api.Infrastructure.CustomResults.Problem))
            .HasPermission(Permissions.SchedulerReportsRead)
            .WithTags(Tags.Scheduler);
    }
}

internal sealed class GetJobsExecutionReportEndpoint : IEndpoint, IOrderedEndpoint
{
    public int Order => 14;

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("scheduler/jobs/report", async (
                string format,
                IQueryHandler<GetJobsExecutionReportQuery, IResult> handler,
                CancellationToken cancellationToken) =>
            (await handler.Handle(new GetJobsExecutionReportQuery(format), cancellationToken))
            .Match(static x => x, Web.Api.Infrastructure.CustomResults.Problem))
            .HasPermission(Permissions.SchedulerReportsRead)
            .WithTags(Tags.Scheduler);
    }
}

