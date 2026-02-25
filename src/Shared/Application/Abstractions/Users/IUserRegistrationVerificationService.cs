namespace Application.Abstractions.Users;

public interface IUserRegistrationVerificationService
{
    Task QueueVerificationAsync(Guid userId, CancellationToken cancellationToken);
    Task<ResendVerificationResult> TryResendVerificationAsync(Guid userId, CancellationToken cancellationToken);
    DateTime GetVerificationExpiryUtc(DateTime nowUtc);
}

public sealed record ResendVerificationResult(
    bool Sent,
    DateTime? VerificationExpiresAtUtc,
    DateTime? CooldownEndsAtUtc,
    string? Error);
