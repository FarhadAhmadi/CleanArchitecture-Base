namespace Application.Users.Register;

public static class PasswordResetTokens
{
    public const string Provider = "password-reset";
    public const string CodeHash = "code-hash";
    public const string ExpiryUtc = "code-expiry-utc";
    public const string LastSentAtUtc = "last-sent-at-utc";
    public const string FailedAttempts = "failed-attempts";
    public const string BlockedUntilUtc = "blocked-until-utc";
}
