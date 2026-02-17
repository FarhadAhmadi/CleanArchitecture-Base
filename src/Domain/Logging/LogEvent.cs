namespace Domain.Logging;

public sealed class LogEvent
{
    public Guid Id { get; set; }
    public string? IdempotencyKey { get; set; }
    public DateTime TimestampUtc { get; set; }
    public LogLevelType Level { get; set; }
    public string Message { get; set; }
    public string SourceService { get; set; }
    public string SourceModule { get; set; }
    public string TraceId { get; set; }
    public string? RequestId { get; set; }
    public string? TenantId { get; set; }
    public string ActorType { get; set; }
    public string? ActorId { get; set; }
    public string Outcome { get; set; }
    public string? SessionId { get; set; }
    public string? Ip { get; set; }
    public string? UserAgent { get; set; }
    public string? DeviceInfo { get; set; }
    public string? HttpMethod { get; set; }
    public string? HttpPath { get; set; }
    public int? HttpStatusCode { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorStackHash { get; set; }
    public string? TagsCsv { get; set; }
    public string? PayloadJson { get; set; }
    public string Checksum { get; set; }
    public bool HasIntegrityIssue { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAtUtc { get; set; }
}
