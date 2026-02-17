namespace Web.Api.Infrastructure;

internal sealed class OperationalAlertingOptions
{
    public const string SectionName = "OperationalAlerting";

    public bool Enabled { get; init; }
    public int PollingIntervalSeconds { get; init; } = 60;
    public int CooldownMinutes { get; init; } = 15;
    public string[] WebhookUrls { get; init; } = [];
    public string? PagerDutyRoutingKey { get; init; }
    public string PagerDutyEventsApiUrl { get; init; } = "https://events.pagerduty.com/v2/enqueue";
}
