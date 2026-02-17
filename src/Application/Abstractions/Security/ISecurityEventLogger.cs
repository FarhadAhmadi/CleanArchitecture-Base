namespace Application.Abstractions.Security;

public interface ISecurityEventLogger
{
    void AuthenticationFailed(string reason, string? subject, string? ip, string? traceId);
    void AuthenticationSucceeded(string subject, string? ip, string? traceId, string method);
    void AuthorizationDenied(string requirement, string? subject, string? ip, string? traceId);
    void AccountLocked(string subject, DateTime lockoutEndUtc, string? ip, string? traceId);
}
