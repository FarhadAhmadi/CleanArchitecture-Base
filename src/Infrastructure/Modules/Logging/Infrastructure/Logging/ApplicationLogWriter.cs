using Application.Abstractions.Data;
using Application.Abstractions.Logging;
using Domain.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Logging;

internal sealed class ApplicationLogWriter(
    ILoggingWriteDbContext dbContext,
    ILogIntegrityService integrityService,
    ILogger<ApplicationLogWriter> logger) : IApplicationLogWriter
{
    public async Task<bool> TryWriteAsync(ApplicationLogEntry entry, CancellationToken cancellationToken)
    {
        bool exists = await dbContext.LogEvents
            .AsNoTracking()
            .AnyAsync(x => x.Id == entry.Id, cancellationToken);

        if (exists)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug(
                    "Application log entry ignored because it already exists. LogId={LogId} Source={SourceModule}",
                    entry.Id,
                    entry.SourceModule);
            }
            return false;
        }

        LogEvent logEvent = new()
        {
            Id = entry.Id,
            TimestampUtc = entry.TimestampUtc,
            Level = entry.Level,
            Message = entry.Message,
            SourceService = entry.SourceService,
            SourceModule = entry.SourceModule,
            TraceId = entry.TraceId,
            ActorType = entry.ActorType,
            ActorId = entry.ActorId,
            Outcome = entry.Outcome,
            TagsCsv = entry.TagsCsv,
            PayloadJson = entry.PayloadJson,
            HasIntegrityIssue = false
        };

        logEvent.Checksum = integrityService.ComputeChecksum(logEvent);
        dbContext.LogEvents.Add(logEvent);
        await dbContext.SaveChangesAsync(cancellationToken);
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Application log entry persisted. LogId={LogId} Level={Level} Source={SourceModule}",
                logEvent.Id,
                logEvent.Level,
                logEvent.SourceModule);
        }
        return true;
    }
}
