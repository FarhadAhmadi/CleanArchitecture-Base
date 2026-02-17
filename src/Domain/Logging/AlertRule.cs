namespace Domain.Logging;

public sealed class AlertRule
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public bool IsEnabled { get; set; }
    public LogLevelType MinimumLevel { get; set; }
    public string? ContainsText { get; set; }
    public int WindowSeconds { get; set; }
    public int ThresholdCount { get; set; }
    public string Action { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}
