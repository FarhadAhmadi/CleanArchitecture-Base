using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Scheduler;
using Domain.Modules.Scheduler;
using Domain.Scheduler;
using Infrastructure.Auditing;
using Microsoft.EntityFrameworkCore;

namespace Application.Scheduler;

public sealed record CreateJobCommand(
    string Name,
    string? Description,
    JobType? Type,
    string? PayloadJson,
    int? MaxRetryAttempts,
    int? RetryBackoffSeconds,
    int? MaxExecutionSeconds,
    int? MaxConsecutiveFailures) : ICommand<IResult>;

internal sealed class CreateJobCommandHandler(
    IApplicationDbContext dbContext,
    IUserContext userContext,
    ISchedulerPayloadValidator payloadValidator,
    IAuditTrailService auditTrailService) : ResultWrappingCommandHandler<CreateJobCommand>
{
    protected override async Task<IResult> HandleCore(CreateJobCommand command, CancellationToken cancellationToken)
    {
        string normalizedName = command.Name.Trim();
        if (string.IsNullOrWhiteSpace(normalizedName))
        {
            return Results.BadRequest(new { message = "Job name is required." });
        }

        bool exists = await dbContext.ScheduledJobs
            .AnyAsync(x => x.Name == normalizedName, cancellationToken);

        if (exists)
        {
            return Results.Conflict(new { message = "A job with this name already exists." });
        }

        JobType resolvedType = command.Type ?? JobType.GenericNoOp;
        SchedulerPayloadValidationResult payloadValidation = payloadValidator.Validate(resolvedType, command.PayloadJson);
        if (!payloadValidation.IsValid)
        {
            return Results.BadRequest(new { message = payloadValidation.Error ?? "Invalid payloadJson." });
        }

        var job = new ScheduledJob
        {
            Id = Guid.NewGuid(),
            Name = normalizedName,
            Description = string.IsNullOrWhiteSpace(command.Description) ? null : command.Description.Trim(),
            Type = resolvedType,
            PayloadJson = payloadValidation.NormalizedPayloadJson,
            Status = JobStatus.Active,
            CreatedAtUtc = DateTime.UtcNow,
            MaxRetryAttempts = Math.Clamp(command.MaxRetryAttempts ?? 3, 1, 10),
            RetryBackoffSeconds = Math.Clamp(command.RetryBackoffSeconds ?? 15, 1, 3600),
            MaxExecutionSeconds = Math.Clamp(command.MaxExecutionSeconds ?? 120, 1, 3600),
            MaxConsecutiveFailures = Math.Clamp(command.MaxConsecutiveFailures ?? 5, 1, 50)
        };

        dbContext.ScheduledJobs.Add(job);
        await dbContext.SaveChangesAsync(cancellationToken);

        await auditTrailService.RecordAsync(
            new AuditRecordRequest(
                userContext.UserId.ToString("N"),
                "scheduler.job.create",
                "ScheduledJob",
                job.Id.ToString("N"),
                $"{{\"name\":\"{job.Name}\",\"type\":\"{job.Type}\"}}"),
            cancellationToken);

        return Results.Ok(new
        {
            job.Id,
            job.Name,
            type = job.Type.ToString(),
            job.PayloadJson,
            status = job.Status.ToString(),
            job.MaxRetryAttempts,
            job.RetryBackoffSeconds,
            job.MaxExecutionSeconds,
            job.MaxConsecutiveFailures
        });
    }
}
