namespace Web.Api.Infrastructure;

internal sealed class TelemetryOptions
{
    public const string SectionName = "Telemetry";

    public string ServiceName { get; init; } = "Web.Api";

    public string? OtlpEndpoint { get; init; }
}
