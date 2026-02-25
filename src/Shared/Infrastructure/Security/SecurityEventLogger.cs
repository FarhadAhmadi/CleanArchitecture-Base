using Application.Abstractions.Security;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Security;

internal sealed class SecurityEventLogger(ILogger<SecurityEventLogger> logger) : ISecurityEventLogger
{
    public void AuthenticationFailed(string reason, string? subject, string? ip, string? traceId)
    {
        if (logger.IsEnabled(LogLevel.Warning))
        {
            logger.LogWarning(
                "Security.AuthenticationFailed Reason={Reason} Subject={Subject} Ip={Ip} TraceId={TraceId}",
                reason,
                subject ?? "unknown",
                ip ?? "unknown",
                traceId ?? "unknown");
        }
    }

    public void AuthenticationSucceeded(string subject, string? ip, string? traceId, string method)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Security.AuthenticationSucceeded Subject={Subject} Method={Method} Ip={Ip} TraceId={TraceId}",
                subject,
                method,
                ip ?? "unknown",
                traceId ?? "unknown");
        }
    }

    public void AuthorizationDenied(string requirement, string? subject, string? ip, string? traceId)
    {
        if (logger.IsEnabled(LogLevel.Warning))
        {
            logger.LogWarning(
                "Security.AuthorizationDenied Requirement={Requirement} Subject={Subject} Ip={Ip} TraceId={TraceId}",
                requirement,
                subject ?? "unknown",
                ip ?? "unknown",
                traceId ?? "unknown");
        }
    }

    public void AccountLocked(string subject, DateTime lockoutEndUtc, string? ip, string? traceId)
    {
        if (logger.IsEnabled(LogLevel.Warning))
        {
            logger.LogWarning(
                "Security.AccountLocked Subject={Subject} LockoutEndUtc={LockoutEndUtc} Ip={Ip} TraceId={TraceId}",
                subject,
                lockoutEndUtc,
                ip ?? "unknown",
                traceId ?? "unknown");
        }
    }
}
