using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Modules.Scheduler;
using Infrastructure.Auditing;
using Microsoft.EntityFrameworkCore;

namespace Application.Scheduler;

public sealed record UpsertJobPermissionsCommand(
    Guid JobId,
    string SubjectType,
    string SubjectValue,
    bool CanRead,
    bool CanManage) : ICommand<IResult>;

public sealed record GetJobPermissionsQuery(Guid JobId) : IQuery<IResult>;

internal sealed class UpsertJobPermissionsCommandHandler(
    IApplicationDbContext dbContext,
    IUserContext userContext,
    IAuditTrailService auditTrailService) : ResultWrappingCommandHandler<UpsertJobPermissionsCommand>
{
    protected override async Task<IResult> HandleCore(UpsertJobPermissionsCommand command, CancellationToken cancellationToken)
    {
        bool jobExists = await dbContext.ScheduledJobs.AnyAsync(x => x.Id == command.JobId, cancellationToken);
        if (!jobExists)
        {
            return Results.NotFound();
        }

        string subjectType = command.SubjectType.Trim();
        string subjectValue = command.SubjectValue.Trim();
        if (string.IsNullOrWhiteSpace(subjectType) || string.IsNullOrWhiteSpace(subjectValue))
        {
            return Results.BadRequest(new { message = "SubjectType and SubjectValue are required." });
        }

        JobPermissionEntry? entry = await dbContext.JobPermissionEntries
            .SingleOrDefaultAsync(
                x => x.JobId == command.JobId &&
                     x.SubjectType == subjectType &&
                     x.SubjectValue == subjectValue,
                cancellationToken);

        if (entry is null)
        {
            entry = new JobPermissionEntry
            {
                Id = Guid.NewGuid(),
                JobId = command.JobId,
                SubjectType = subjectType,
                SubjectValue = subjectValue,
                CreatedAtUtc = DateTime.UtcNow
            };
            dbContext.JobPermissionEntries.Add(entry);
        }

        entry.CanRead = command.CanRead;
        entry.CanManage = command.CanManage;

        await dbContext.SaveChangesAsync(cancellationToken);

        await auditTrailService.RecordAsync(
            new AuditRecordRequest(
                userContext.UserId.ToString("N"),
                "scheduler.permissions.upsert",
                "JobPermissionEntry",
                entry.Id.ToString("N"),
                $"{{\"jobId\":\"{command.JobId:N}\",\"subjectType\":\"{subjectType}\"}}"),
            cancellationToken);

        return Results.Ok(new
        {
            entry.Id,
            entry.JobId,
            entry.SubjectType,
            entry.SubjectValue,
            entry.CanRead,
            entry.CanManage
        });
    }
}

internal sealed class GetJobPermissionsQueryHandler(IApplicationReadDbContext readDbContext) : ResultWrappingQueryHandler<GetJobPermissionsQuery>
{
    protected override async Task<IResult> HandleCore(GetJobPermissionsQuery query, CancellationToken cancellationToken)
    {
        List<object> items = await readDbContext.JobPermissionEntries
            .Where(x => x.JobId == query.JobId)
            .OrderBy(x => x.SubjectType)
            .ThenBy(x => x.SubjectValue)
            .Select(x => new
            {
                x.Id,
                x.JobId,
                x.SubjectType,
                x.SubjectValue,
                x.CanRead,
                x.CanManage
            })
            .Cast<object>()
            .ToListAsync(cancellationToken);

        return Results.Ok(new { total = items.Count, items });
    }
}

