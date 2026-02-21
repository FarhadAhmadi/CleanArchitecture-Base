using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Scheduler;
using Domain.Modules.Scheduler;
using Domain.Scheduler;
using Infrastructure.Auditing;
using Microsoft.EntityFrameworkCore;

namespace Application.Scheduler;

public sealed record UpdateJobCommand(
    Guid JobId,
    string Name,
    string? Description,
    JobType? Type,
    string? PayloadJson,
    int? MaxRetryAttempts,
    int? RetryBackoffSeconds,
    int? MaxExecutionSeconds,
    int? MaxConsecutiveFailures) : ICommand<IResult>;

internal sealed class UpdateJobCommandHandler(
    IApplicationDbContext dbContext,
    IUserContext userContext,
    ISchedulerPayloadValidator payloadValidator,
    IAuditTrailService auditTrailService) : ResultWrappingCommandHandler<UpdateJobCommand>
{
    protected override async Task<IResult> HandleCore(UpdateJobCommand command, CancellationToken cancellationToken)
    {
        ScheduledJob? job = await dbContext.ScheduledJobs
            .SingleOrDefaultAsync(x => x.Id == command.JobId, cancellationToken);

        if (job is null)
        {
            return Results.NotFound();
        }

        string normalizedName = command.Name.Trim();
        if (string.IsNullOrWhiteSpace(normalizedName))
        {
            return Results.BadRequest(new { message = "Job name is required." });
        }

        bool duplicate = await dbContext.ScheduledJobs
            .AnyAsync(x => x.Id != command.JobId && x.Name == normalizedName, cancellationToken);

        if (duplicate)
        {
            return Results.Conflict(new { message = "A job with this name already exists." });
        }

        job.Name = normalizedName;
        job.Description = string.IsNullOrWhiteSpace(command.Description) ? null : command.Description.Trim();
        if (command.Type.HasValue)
        {
            job.Type = command.Type.Value;
        }

        SchedulerPayloadValidationResult payloadValidation = payloadValidator.Validate(job.Type, command.PayloadJson);
        if (!payloadValidation.IsValid)
        {
            return Results.BadRequest(new { message = payloadValidation.Error ?? "Invalid payloadJson." });
        }

        job.PayloadJson = payloadValidation.NormalizedPayloadJson;
        if (command.MaxRetryAttempts.HasValue)
        {
            job.MaxRetryAttempts = Math.Clamp(command.MaxRetryAttempts.Value, 1, 10);
        }

        if (command.RetryBackoffSeconds.HasValue)
        {
            job.RetryBackoffSeconds = Math.Clamp(command.RetryBackoffSeconds.Value, 1, 3600);
        }

        if (command.MaxExecutionSeconds.HasValue)
        {
            job.MaxExecutionSeconds = Math.Clamp(command.MaxExecutionSeconds.Value, 1, 3600);
        }

        if (command.MaxConsecutiveFailures.HasValue)
        {
            job.MaxConsecutiveFailures = Math.Clamp(command.MaxConsecutiveFailures.Value, 1, 50);
        }

        job.Raise(new SchedulerJobUpdatedDomainEvent(job.Id, job.Name, job.Type));

        await dbContext.SaveChangesAsync(cancellationToken);

        await auditTrailService.RecordAsync(
            new AuditRecordRequest(
                userContext.UserId.ToString("N"),
                "scheduler.job.update",
                "ScheduledJob",
                job.Id.ToString("N"),
                $"{{\"name\":\"{job.Name}\",\"type\":\"{job.Type}\"}}"),
            cancellationToken);

        return Results.Ok(new
        {
            job.Id,
            job.Name,
            job.Description,
            type = job.Type.ToString(),
            job.PayloadJson,
            job.MaxRetryAttempts,
            job.RetryBackoffSeconds,
            job.MaxExecutionSeconds,
            job.MaxConsecutiveFailures
        });
    }
}
