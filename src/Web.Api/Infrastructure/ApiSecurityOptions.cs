namespace Web.Api.Infrastructure;

internal sealed class ApiSecurityOptions
{
    public const string SectionName = "ApiSecurity";

    public string[] AllowedOrigins { get; init; } = [];

    public int MaxRequestBodySizeMb { get; init; } = 10;

    public int RequestTimeoutSeconds { get; init; } = 30;

    public int RateLimitPermitLimit { get; init; } = 100;

    public int RateLimitWindowSeconds { get; init; } = 60;

    public int PerUserRateLimitPermitLimit { get; init; } = 60;

    public int PerIpRateLimitPermitLimit { get; init; } = 120;

    public int PublicFileLinkRateLimitPermitLimit { get; init; } = 20;

    public int PublicFileLinkRateLimitWindowSeconds { get; init; } = 60;

    public bool EnableSecurityHeaders { get; init; } = true;

    public bool UseHsts { get; init; } = true;

    public bool HideServerHeader { get; init; } = true;

    public int MaxRequestHeadersTotalSizeKb { get; init; } = 32;

    public int MaxRequestHeaderCount { get; init; } = 100;

    public int RequestHeadersTimeoutSeconds { get; init; } = 15;

    public bool EnforceJsonAcceptHeader { get; init; }
}

