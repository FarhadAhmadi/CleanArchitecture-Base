using System.Text;
using System.Globalization;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;

namespace Application.Scheduler;

public sealed record GetJobsExecutionReportQuery(string Format) : IQuery<IResult>;

internal sealed class GetJobsExecutionReportQueryHandler(IApplicationReadDbContext readDbContext) : ResultWrappingQueryHandler<GetJobsExecutionReportQuery>
{
    protected override async Task<IResult> HandleCore(GetJobsExecutionReportQuery query, CancellationToken cancellationToken)
    {
        string normalizedFormat = (query.Format ?? "csv").Trim().ToUpperInvariant();
        if (normalizedFormat is not ("CSV" or "PDF"))
        {
            return Results.BadRequest(new { message = "Format must be csv or pdf." });
        }

        List<ReportRow> rows = await readDbContext.JobExecutions
            .OrderByDescending(x => x.StartedAtUtc)
            .Take(5000)
            .Select(x => new ReportRow(
                x.JobId,
                x.Status.ToString(),
                x.TriggeredBy,
                x.NodeId,
                x.ScheduledAtUtc,
                x.StartedAtUtc,
                x.FinishedAtUtc,
                x.DurationMs,
                x.Attempt,
                x.MaxAttempts,
                x.IsDeadLetter,
                x.Error))
            .ToListAsync(cancellationToken);

        if (normalizedFormat == "PDF")
        {
            byte[] content = Encoding.UTF8.GetBytes("Scheduler report placeholder. Use csv for structured export.");
            return Results.File(content, "application/pdf", "scheduler-report.pdf");
        }

        var builder = new StringBuilder();
        builder.AppendLine("JobId,Status,TriggeredBy,NodeId,ScheduledAtUtc,StartedAtUtc,FinishedAtUtc,DurationMs,Attempt,MaxAttempts,IsDeadLetter,Error");
        foreach (ReportRow row in rows)
        {
            string error = (row.Error ?? string.Empty).Replace(",", " ");
            string line = string.Format(
                CultureInfo.InvariantCulture,
                "{0},{1},{2},{3},{4:O},{5:O},{6:O},{7},{8},{9},{10},{11}",
                row.JobId,
                row.Status,
                row.TriggeredBy,
                row.NodeId,
                row.ScheduledAtUtc,
                row.StartedAtUtc,
                row.FinishedAtUtc,
                row.DurationMs,
                row.Attempt,
                row.MaxAttempts,
                row.IsDeadLetter,
                error);
            builder.AppendLine(line);
        }

        byte[] csvBytes = Encoding.UTF8.GetBytes(builder.ToString());
        return Results.File(csvBytes, "text/csv", "scheduler-report.csv");
    }

    private sealed record ReportRow(
        Guid JobId,
        string Status,
        string TriggeredBy,
        string? NodeId,
        DateTime ScheduledAtUtc,
        DateTime StartedAtUtc,
        DateTime? FinishedAtUtc,
        int DurationMs,
        int Attempt,
        int MaxAttempts,
        bool IsDeadLetter,
        string? Error);
}
