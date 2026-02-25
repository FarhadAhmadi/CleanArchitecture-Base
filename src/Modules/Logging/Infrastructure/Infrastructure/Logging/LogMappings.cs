using Domain.Logging;

namespace Infrastructure.Logging;

internal static class LogMappings
{
    internal static LogEvent ToEntity(this IngestLogRequest request, string? idempotencyKey)
    {
        return new LogEvent
        {
            Id = Guid.NewGuid(),
            IdempotencyKey = idempotencyKey,
            TimestampUtc = request.TimestampUtc == default ? DateTime.UtcNow : request.TimestampUtc,
            Level = request.Level,
            Message = request.Message,
            SourceService = request.SourceService,
            SourceModule = request.SourceModule,
            TraceId = string.IsNullOrWhiteSpace(request.TraceId) ? Guid.NewGuid().ToString("N") : request.TraceId!,
            RequestId = request.RequestId,
            TenantId = request.TenantId,
            ActorType = request.ActorType,
            ActorId = request.ActorId,
            Outcome = request.Outcome,
            SessionId = request.SessionId,
            Ip = request.Ip,
            UserAgent = request.UserAgent,
            DeviceInfo = request.DeviceInfo,
            HttpMethod = request.HttpMethod,
            HttpPath = request.HttpPath,
            HttpStatusCode = request.HttpStatusCode,
            ErrorCode = request.ErrorCode,
            ErrorStackHash = request.ErrorStackHash,
            TagsCsv = request.Tags.Count == 0 ? null : string.Join(',', request.Tags),
            PayloadJson = request.PayloadJson
        };
    }
}
