using System.Text.Json;
using Domain.Logging;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Logging;

internal sealed class LogIngestionService(
    ApplicationDbContext dbContext,
    ILogSanitizer sanitizer,
    ILogIngestionQueue queue,
    IAlertDispatchQueue alertQueue,
    ILogIntegrityService integrityService,
    ILogger<LogIngestionService> logger) : ILogIngestionService
{
    public async Task<IngestResult> IngestAsync(
        IngestLogRequest request,
        string? idempotencyKey,
        CancellationToken cancellationToken)
    {
        IngestLogRequest sanitized = sanitizer.Sanitize(request);

        if (!string.IsNullOrWhiteSpace(idempotencyKey))
        {
            LogEvent? existing = await dbContext.LogEvents
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.IdempotencyKey == idempotencyKey, cancellationToken);

            if (existing is not null)
            {
#pragma warning disable CA1873 // Avoid potentially expensive logging
                logger.LogInformation(
                    "Duplicate log event ignored. EventId={EventId} IdempotencyKey={IdempotencyKey} SourceService={SourceService}",
                    existing.Id,
                    idempotencyKey,
                    existing.SourceService);
#pragma warning restore CA1873 // Avoid potentially expensive logging

                return new IngestResult
                {
                    EventId = existing.Id,
                    IsDuplicate = true
                };
            }
        }

        LogEvent entity = sanitized.ToEntity(idempotencyKey);
        entity.PayloadJson = SanitizePayloadLength(entity.PayloadJson);
        entity.Checksum = integrityService.ComputeChecksum(entity);
        entity.HasIntegrityIssue = false;

        try
        {
            dbContext.LogEvents.Add(entity);
            await dbContext.SaveChangesAsync(cancellationToken);

#pragma warning disable CA1873 // Avoid potentially expensive logging
            logger.LogInformation(
                "Log event stored. EventId={EventId} Level={Level} ActorId={ActorId} TraceId={TraceId} Source={SourceService}/{SourceModule}",
                entity.Id,
                entity.Level,
                entity.ActorId,
                entity.TraceId,
                entity.SourceService,
                entity.SourceModule);
#pragma warning restore CA1873 // Avoid potentially expensive logging

            await EvaluateAlertRulesAsync(entity, cancellationToken);

            return new IngestResult { EventId = entity.Id };
        }
        catch (Exception ex)
        {
            bool queued = queue.TryEnqueue(entity);
            logger.LogWarning(
                ex,
                "Failed to persist log event and queued for retry={Queued}. EventId={EventId} Level={Level}",
                queued,
                entity.Id,
                entity.Level);

            return new IngestResult
            {
                EventId = entity.Id,
                QueuedForRetry = queued
            };
        }
    }

    private async Task EvaluateAlertRulesAsync(LogEvent logEvent, CancellationToken cancellationToken)
    {
        List<AlertRule> rules = await dbContext.AlertRules
            .Where(x => x.IsEnabled && x.MinimumLevel <= logEvent.Level)
            .ToListAsync(cancellationToken);

        foreach (AlertRule rule in rules)
        {
            bool textOk = string.IsNullOrWhiteSpace(rule.ContainsText) ||
                          logEvent.Message.Contains(rule.ContainsText, StringComparison.OrdinalIgnoreCase);

            if (!textOk)
            {
                continue;
            }

            DateTime from = logEvent.TimestampUtc.AddSeconds(-rule.WindowSeconds);
            int matchedCount = await dbContext.LogEvents
                .Where(x => !x.IsDeleted &&
                            x.TimestampUtc >= from &&
                            x.TimestampUtc <= logEvent.TimestampUtc &&
                            x.Level >= rule.MinimumLevel &&
                            (string.IsNullOrWhiteSpace(rule.ContainsText) ||
                             x.Message.Contains(rule.ContainsText)))
                .CountAsync(cancellationToken);

            if (matchedCount < Math.Max(1, rule.ThresholdCount))
            {
                continue;
            }

            AlertIncident incident = new()
            {
                Id = Guid.NewGuid(),
                RuleId = rule.Id,
                TriggerEventId = logEvent.Id,
                TriggeredAtUtc = DateTime.UtcNow,
                Status = "Queued"
            };

            dbContext.AlertIncidents.Add(incident);
            await dbContext.SaveChangesAsync(cancellationToken);
            alertQueue.TryEnqueue(incident.Id);

            logger.LogWarning(
                "Alert rule triggered. RuleId={RuleId} RuleName={RuleName} EventId={EventId} MatchedCount={MatchedCount}",
                rule.Id,
                rule.Name,
                logEvent.Id,
                matchedCount);
        }
    }

    private static string? SanitizePayloadLength(string? payloadJson)
    {
        if (string.IsNullOrWhiteSpace(payloadJson))
        {
            return null;
        }

        return payloadJson.Length <= 32768
            ? payloadJson
            : JsonSerializer.Serialize(new { truncated = true, length = payloadJson.Length });
    }
}
