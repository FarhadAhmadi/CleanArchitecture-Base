namespace Application.Abstractions.Security;

public sealed class AuthSecurityOptions
{
    public const string SectionName = "AuthSecurity";

    public int MaxFailedLoginAttempts { get; init; } = 5;
    public int LockoutMinutes { get; init; } = 15;
}
