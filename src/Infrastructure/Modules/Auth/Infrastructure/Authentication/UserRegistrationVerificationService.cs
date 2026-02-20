using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Application.Abstractions.Security;
using Application.Abstractions.Users;
using Application.Users.Register;
using Domain.Notifications;
using Domain.Users;
using Infrastructure.Database;
using Infrastructure.Notifications;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Authentication;

internal sealed class UserRegistrationVerificationService(
    UserManager<User> userManager,
    ApplicationDbContext dbContext,
    AuthSecurityOptions authSecurityOptions,
    NotificationOptions notificationOptions,
    NotificationSensitiveDataProtector protector,
    NotificationTemplateRenderer templateRenderer) : IUserRegistrationVerificationService
{
    public DateTime GetVerificationExpiryUtc(DateTime nowUtc)
    {
        int expiresInSeconds = Math.Max(30, authSecurityOptions.EmailVerificationCodeExpiresInSeconds);
        return nowUtc.AddSeconds(expiresInSeconds);
    }

    public async Task QueueVerificationAsync(Guid userId, CancellationToken cancellationToken)
    {
        _ = await QueueVerificationInternalAsync(userId, enforceCooldown: false, cancellationToken);
    }

    public async Task<ResendVerificationResult> TryResendVerificationAsync(Guid userId, CancellationToken cancellationToken)
    {
        (bool queued, DateTime? expiresAtUtc, DateTime? cooldownEndsAtUtc, string? error) =
            await QueueVerificationInternalAsync(userId, enforceCooldown: true, cancellationToken);

        return new ResendVerificationResult(queued, expiresAtUtc, cooldownEndsAtUtc, error);
    }

    private async Task<(bool Queued, DateTime? ExpiresAtUtc, DateTime? CooldownEndsAtUtc, string? Error)> QueueVerificationInternalAsync(
        Guid userId,
        bool enforceCooldown,
        CancellationToken cancellationToken)
    {
        User? user = await userManager.FindByIdAsync(userId.ToString("D"));
        if (user is null || string.IsNullOrWhiteSpace(user.Email))
        {
            return (false, null, null, "UserNotFound");
        }

        if (user.EmailConfirmed)
        {
            return (false, null, null, "AlreadyVerified");
        }

        DateTime nowUtc = DateTime.UtcNow;
        int resendCooldownSeconds = Math.Max(10, authSecurityOptions.EmailVerificationResendCooldownSeconds);

        if (enforceCooldown)
        {
            string? lastSentText = await userManager.GetAuthenticationTokenAsync(
                user,
                EmailVerificationTokens.Provider,
                EmailVerificationTokens.LastSentAtUtc);

            if (TryParseUtc(lastSentText, out DateTime lastSentAtUtc))
            {
                DateTime cooldownEndsAtUtc = lastSentAtUtc.AddSeconds(resendCooldownSeconds);
                if (cooldownEndsAtUtc > nowUtc)
                {
                    return (false, null, cooldownEndsAtUtc, "CooldownActive");
                }
            }
        }

        string code = RandomNumberGenerator.GetInt32(100000, 1000000).ToString(CultureInfo.InvariantCulture);
        string codeHash = ComputeSha256(code);
        DateTime verificationExpiresAtUtc = GetVerificationExpiryUtc(nowUtc);

        await userManager.SetAuthenticationTokenAsync(
            user,
            EmailVerificationTokens.Provider,
            EmailVerificationTokens.CodeHash,
            codeHash);

        await userManager.SetAuthenticationTokenAsync(
            user,
            EmailVerificationTokens.Provider,
            EmailVerificationTokens.ExpiryUtc,
            verificationExpiresAtUtc.ToString("O"));

        await userManager.SetAuthenticationTokenAsync(
            user,
            EmailVerificationTokens.Provider,
            EmailVerificationTokens.LastSentAtUtc,
            nowUtc.ToString("O"));

        await userManager.SetAuthenticationTokenAsync(
            user,
            EmailVerificationTokens.Provider,
            EmailVerificationTokens.FailedAttempts,
            "0");

        await userManager.RemoveAuthenticationTokenAsync(
            user,
            EmailVerificationTokens.Provider,
            EmailVerificationTokens.BlockedUntilUtc);

        var variables = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["AppName"] = "CleanArchitecture.WebApi",
            ["FirstName"] = user.FirstName,
            ["LastName"] = user.LastName,
            ["VerificationCode"] = code,
            ["ExpiresAtUtc"] = verificationExpiresAtUtc.ToString("O")
        };

        RenderedNotificationTemplate? rendered = await templateRenderer.TryRenderAsync(
            NotificationTemplateCatalog.UserRegistrationVerificationEmail,
            "fa-IR",
            NotificationChannel.Email,
            variables,
            cancellationToken);

        dbContext.NotificationMessages.Add(new NotificationMessage
        {
            Id = Guid.NewGuid(),
            CreatedByUserId = user.Id,
            Channel = NotificationChannel.Email,
            Priority = NotificationPriority.High,
            Status = NotificationStatus.Pending,
            RecipientEncrypted = protector.Protect(user.Email),
            RecipientHash = NotificationSensitiveDataProtector.ComputeDeterministicHash(user.Email),
            Subject = rendered?.Subject ?? "تایید ثبت نام",
            Body = rendered?.Body ?? $"کد تایید شما: {code}",
            Language = "fa-IR",
            TemplateId = rendered?.TemplateId,
            CreatedAtUtc = DateTime.UtcNow,
            MaxRetryCount = Math.Max(1, notificationOptions.MaxRetries)
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        return (true, verificationExpiresAtUtc, nowUtc.AddSeconds(resendCooldownSeconds), null);
    }

    private static string ComputeSha256(string value)
    {
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(hash);
    }

    private static bool TryParseUtc(string? value, out DateTime utcValue)
    {
        if (DateTime.TryParseExact(
            value,
            "O",
            CultureInfo.InvariantCulture,
            DateTimeStyles.RoundtripKind,
            out DateTime parsed))
        {
            utcValue = DateTime.SpecifyKind(parsed, DateTimeKind.Utc);
            return true;
        }

        utcValue = default;
        return false;
    }
}
