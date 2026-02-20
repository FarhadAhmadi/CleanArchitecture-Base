namespace Application.Abstractions.Users;

public interface IUserPasswordResetService
{
    Task<PasswordResetRequestResult> RequestResetAsync(string email, CancellationToken cancellationToken);
}

public sealed record PasswordResetRequestResult(
    bool Queued,
    DateTime? ExpiresAtUtc,
    DateTime? CooldownEndsAtUtc);
