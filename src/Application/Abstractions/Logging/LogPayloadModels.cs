using Domain.Logging;

namespace Infrastructure.Logging;

public sealed class IngestLogRequest
{
    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
    public LogLevelType Level { get; set; } = LogLevelType.Info;
    public string Message { get; set; }
    public string SourceService { get; set; } = "Web.Api";
    public string SourceModule { get; set; } = "unknown";
    public string? TraceId { get; set; }
    public string? RequestId { get; set; }
    public string? TenantId { get; set; }
    public string ActorType { get; set; } = "system";
    public string? ActorId { get; set; }
    public string Outcome { get; set; } = "unknown";
    public string? SessionId { get; set; }
    public string? Ip { get; set; }
    public string? UserAgent { get; set; }
    public string? DeviceInfo { get; set; }
    public string? HttpMethod { get; set; }
    public string? HttpPath { get; set; }
    public int? HttpStatusCode { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorStackHash { get; set; }
    public List<string> Tags { get; set; } = [];
    public string? PayloadJson { get; set; }
}

public sealed class IngestResult
{
    public Guid EventId { get; set; }
    public bool QueuedForRetry { get; set; }
    public bool IsDuplicate { get; set; }
}
