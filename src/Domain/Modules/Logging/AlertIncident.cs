using SharedKernel;

namespace Domain.Logging;

public sealed class AlertIncident : Entity
{
    public Guid RuleId { get; set; }
    public Guid TriggerEventId { get; set; }
    public DateTime TriggeredAtUtc { get; set; }
    public string Status { get; set; }
    public int RetryCount { get; set; }
    public DateTime? NextRetryAtUtc { get; set; }
    public string? LastError { get; set; }
}
