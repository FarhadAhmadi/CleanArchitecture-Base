namespace Application.Abstractions.Security;

public sealed class AuthSecurityOptions
{
    public const string SectionName = "AuthSecurity";

    public int MaxFailedLoginAttempts { get; init; } = 5;
    public int LockoutMinutes { get; init; } = 15;
    public int EmailVerificationCodeExpiresInSeconds { get; init; } = 120;
    public int EmailVerificationResendCooldownSeconds { get; init; } = 60;
    public int EmailVerificationMaxFailedAttempts { get; init; } = 5;
    public int EmailVerificationBlockMinutes { get; init; } = 15;
    public int PasswordResetCodeExpiresInSeconds { get; init; } = 600;
    public int PasswordResetRequestCooldownSeconds { get; init; } = 60;
}
