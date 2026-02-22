using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Application.Abstractions.Notifications;
using Application.Abstractions.Security;
using Application.Abstractions.Users;
using Application.Users.Register;
using Domain.Modules.Notifications;
using Domain.Notifications;
using Domain.Users;
using Infrastructure.Notifications;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Authentication;

internal sealed class UserPasswordResetService(
    UserManager<User> userManager,
    AuthSecurityOptions authSecurityOptions,
    NotificationOptions notificationOptions,
    INotificationMessageWriter notificationMessageWriter,
    NotificationTemplateRenderer templateRenderer,
    ILogger<UserPasswordResetService> logger) : IUserPasswordResetService
{
    public async Task<PasswordResetRequestResult> RequestResetAsync(string email, CancellationToken cancellationToken)
    {
        User? user = await userManager.FindByEmailAsync(email);
        if (user is null || string.IsNullOrWhiteSpace(user.Email))
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug("Password reset request ignored because user/email was not found. Email={Email}", email);
            }
            return new PasswordResetRequestResult(false, null, null);
        }

        DateTime nowUtc = DateTime.UtcNow;
        int resendCooldownSeconds = Math.Max(10, authSecurityOptions.PasswordResetRequestCooldownSeconds);

        string? lastSentText = await userManager.GetAuthenticationTokenAsync(
            user,
            PasswordResetTokens.Provider,
            PasswordResetTokens.LastSentAtUtc);

        if (TryParseUtc(lastSentText, out DateTime lastSentAtUtc))
        {
            DateTime cooldownEndsAtUtc = lastSentAtUtc.AddSeconds(resendCooldownSeconds);
            if (cooldownEndsAtUtc > nowUtc)
            {
                if (logger.IsEnabled(LogLevel.Debug))
                {
                    logger.LogDebug("Password reset request blocked by cooldown. UserId={UserId} CooldownEndsAtUtc={CooldownEndsAtUtc}", user.Id, cooldownEndsAtUtc);
                }
                return new PasswordResetRequestResult(false, null, cooldownEndsAtUtc);
            }
        }

        string code = RandomNumberGenerator.GetInt32(100000, 1000000).ToString(CultureInfo.InvariantCulture);
        DateTime expiresAtUtc = nowUtc.AddSeconds(Math.Max(60, authSecurityOptions.PasswordResetCodeExpiresInSeconds));

        await userManager.SetAuthenticationTokenAsync(
            user,
            PasswordResetTokens.Provider,
            PasswordResetTokens.CodeHash,
            ComputeSha256(code));

        await userManager.SetAuthenticationTokenAsync(
            user,
            PasswordResetTokens.Provider,
            PasswordResetTokens.ExpiryUtc,
            expiresAtUtc.ToString("O"));

        await userManager.SetAuthenticationTokenAsync(
            user,
            PasswordResetTokens.Provider,
            PasswordResetTokens.LastSentAtUtc,
            nowUtc.ToString("O"));

        await userManager.SetAuthenticationTokenAsync(
            user,
            PasswordResetTokens.Provider,
            PasswordResetTokens.FailedAttempts,
            "0");

        await userManager.RemoveAuthenticationTokenAsync(
            user,
            PasswordResetTokens.Provider,
            PasswordResetTokens.BlockedUntilUtc);

        var variables = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["AppName"] = "CleanArchitecture.WebApi",
            ["FirstName"] = user.FirstName,
            ["LastName"] = user.LastName,
            ["ResetCode"] = code,
            ["ExpiresAtUtc"] = expiresAtUtc.ToString("O")
        };

        RenderedNotificationTemplate? rendered = await templateRenderer.TryRenderAsync(
            NotificationTemplateCatalog.UserPasswordResetEmail,
            "fa-IR",
            NotificationChannel.Email,
            variables,
            cancellationToken);

        bool queued = await notificationMessageWriter.TryQueueAsync(
            new NotificationMessageDraft(
                Id: Guid.NewGuid(),
                CreatedByUserId: user.Id,
                Channel: NotificationChannel.Email,
                Priority: NotificationPriority.High,
                Status: NotificationStatus.Pending,
                RecipientRaw: user.Email,
                Subject: rendered?.Subject ?? "Password reset code",
                Body: rendered?.Body ?? $"Your password reset code is: {code}",
                Language: "fa-IR",
                CreatedAtUtc: nowUtc,
                MaxRetryCount: Math.Max(1, notificationOptions.MaxRetries),
                TemplateId: rendered?.TemplateId),
            cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Password reset notification processed. UserId={UserId} NotificationQueued={Queued} ExpiresAtUtc={ExpiresAtUtc}",
                user.Id,
                queued,
                expiresAtUtc);
        }

        return new PasswordResetRequestResult(true, expiresAtUtc, nowUtc.AddSeconds(resendCooldownSeconds));
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
