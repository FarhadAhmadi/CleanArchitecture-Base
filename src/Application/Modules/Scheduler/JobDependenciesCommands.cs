using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Modules.Scheduler;
using Infrastructure.Auditing;
using Microsoft.EntityFrameworkCore;

namespace Application.Scheduler;

public sealed record AddJobDependencyCommand(Guid JobId, Guid DependsOnJobId) : ICommand<IResult>;
public sealed record RemoveJobDependencyCommand(Guid JobId, Guid DependsOnJobId) : ICommand<IResult>;
public sealed record GetJobDependenciesQuery(Guid JobId) : IQuery<IResult>;

internal sealed class AddJobDependencyCommandHandler(
    IApplicationDbContext dbContext,
    IUserContext userContext,
    IAuditTrailService auditTrailService) : ResultWrappingCommandHandler<AddJobDependencyCommand>
{
    protected override async Task<IResult> HandleCore(AddJobDependencyCommand command, CancellationToken cancellationToken)
    {
        if (command.JobId == command.DependsOnJobId)
        {
            return Results.BadRequest(new { message = "A job cannot depend on itself." });
        }

        bool jobExists = await dbContext.ScheduledJobs.AnyAsync(x => x.Id == command.JobId, cancellationToken);
        bool parentExists = await dbContext.ScheduledJobs.AnyAsync(x => x.Id == command.DependsOnJobId, cancellationToken);
        if (!jobExists || !parentExists)
        {
            return Results.NotFound();
        }

        bool reverseEdgeExists = await dbContext.JobDependencies
            .AnyAsync(x => x.JobId == command.DependsOnJobId && x.DependsOnJobId == command.JobId, cancellationToken);
        if (reverseEdgeExists)
        {
            return Results.BadRequest(new { message = "Direct circular dependency is not allowed." });
        }

        bool exists = await dbContext.JobDependencies
            .AnyAsync(x => x.JobId == command.JobId && x.DependsOnJobId == command.DependsOnJobId, cancellationToken);

        if (exists)
        {
            return Results.Ok(new { added = false, reason = "already_exists" });
        }

        var entry = new JobDependency
        {
            Id = Guid.NewGuid(),
            JobId = command.JobId,
            DependsOnJobId = command.DependsOnJobId,
            CreatedAtUtc = DateTime.UtcNow
        };

        dbContext.JobDependencies.Add(entry);
        await dbContext.SaveChangesAsync(cancellationToken);

        await auditTrailService.RecordAsync(
            new AuditRecordRequest(
                userContext.UserId.ToString("N"),
                "scheduler.dependencies.add",
                "JobDependency",
                entry.Id.ToString("N"),
                $"{{\"jobId\":\"{command.JobId:N}\",\"dependsOnJobId\":\"{command.DependsOnJobId:N}\"}}"),
            cancellationToken);

        return Results.Ok(new { added = true, entry.Id, entry.JobId, entry.DependsOnJobId });
    }
}

internal sealed class RemoveJobDependencyCommandHandler(
    IApplicationDbContext dbContext,
    IUserContext userContext,
    IAuditTrailService auditTrailService) : ResultWrappingCommandHandler<RemoveJobDependencyCommand>
{
    protected override async Task<IResult> HandleCore(RemoveJobDependencyCommand command, CancellationToken cancellationToken)
    {
        JobDependency? entry = await dbContext.JobDependencies
            .SingleOrDefaultAsync(
                x => x.JobId == command.JobId && x.DependsOnJobId == command.DependsOnJobId,
                cancellationToken);

        if (entry is null)
        {
            return Results.NotFound();
        }

        dbContext.JobDependencies.Remove(entry);
        await dbContext.SaveChangesAsync(cancellationToken);

        await auditTrailService.RecordAsync(
            new AuditRecordRequest(
                userContext.UserId.ToString("N"),
                "scheduler.dependencies.remove",
                "JobDependency",
                entry.Id.ToString("N"),
                "{}"),
            cancellationToken);

        return Results.Ok(new { removed = true });
    }
}

internal sealed class GetJobDependenciesQueryHandler(IApplicationReadDbContext readDbContext) : ResultWrappingQueryHandler<GetJobDependenciesQuery>
{
    protected override async Task<IResult> HandleCore(GetJobDependenciesQuery query, CancellationToken cancellationToken)
    {
        List<object> items = await readDbContext.JobDependencies
            .Where(x => x.JobId == query.JobId)
            .Select(x => new
            {
                x.Id,
                x.JobId,
                x.DependsOnJobId,
                x.CreatedAtUtc
            })
            .Cast<object>()
            .ToListAsync(cancellationToken);

        return Results.Ok(new { total = items.Count, items });
    }
}

