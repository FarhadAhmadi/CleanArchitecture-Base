namespace Web.Api.Infrastructure;

internal sealed class ApiSecurityOptions
{
    public const string SectionName = "ApiSecurity";

    public string[] AllowedOrigins { get; init; } = [];

    public int MaxRequestBodySizeMb { get; init; } = 10;

    public int RequestTimeoutSeconds { get; init; } = 30;

    public int RateLimitPermitLimit { get; init; } = 100;

    public int RateLimitWindowSeconds { get; init; } = 60;
}
