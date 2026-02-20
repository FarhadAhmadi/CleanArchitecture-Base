using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Application.Abstractions.Security;
using Application.Abstractions.Users;
using Application.Users.Register;
using Domain.Users;
using Infrastructure.Auditing;
using Microsoft.AspNetCore.Identity;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Users;

internal sealed class ForgotPassword : IEndpoint
{
    public sealed record RequestReset(string Email);
    public sealed record RequestConfirm(string Email, string Code, string NewPassword);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("users/forgot-password", RequestResetAsync)
            .WithTags(Tags.Users);

        app.MapPost("users/reset-password", ConfirmResetAsync)
            .WithTags(Tags.Users);
    }

    private static async Task<IResult> RequestResetAsync(
        RequestReset request,
        IUserPasswordResetService passwordResetService,
        IAuditTrailService auditTrailService,
        CancellationToken cancellationToken)
    {
        string email = InputSanitizer.SanitizeEmail(request.Email) ?? string.Empty;
        if (string.IsNullOrWhiteSpace(email))
        {
            return Results.BadRequest(new { error = "Email is required." });
        }

        PasswordResetRequestResult result = await passwordResetService.RequestResetAsync(email, cancellationToken);

        await auditTrailService.RecordAsync(
            new AuditRecordRequest(
                email,
                result.Queued ? "auth.password-reset.requested" : "auth.password-reset.request-ignored",
                "User",
                email,
                "{\"scope\":\"password-reset-request\"}"),
            cancellationToken);

        if (!result.Queued && result.CooldownEndsAtUtc.HasValue)
        {
            return Results.Json(
                new
                {
                    requested = false,
                    error = "Cooldown is active.",
                    resendAvailableAtUtc = result.CooldownEndsAtUtc
                },
                statusCode: StatusCodes.Status429TooManyRequests);
        }

        return Results.Ok(new
        {
            requested = true,
            resetCodeExpiresAtUtc = result.ExpiresAtUtc,
            resendAvailableAtUtc = result.CooldownEndsAtUtc
        });
    }

    private static async Task<IResult> ConfirmResetAsync(
        RequestConfirm request,
        UserManager<User> userManager,
        AuthSecurityOptions authSecurityOptions,
        IAuditTrailService auditTrailService,
        CancellationToken cancellationToken)
    {
        string email = InputSanitizer.SanitizeEmail(request.Email) ?? string.Empty;
        string code = InputSanitizer.SanitizeIdentifier(request.Code, 12) ?? string.Empty;

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(request.NewPassword))
        {
            return Results.BadRequest(new { error = "Email, code and new password are required." });
        }

        User? user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            return Results.BadRequest(new { error = "Reset code is invalid." });
        }

        string? blockedUntilText = await userManager.GetAuthenticationTokenAsync(
            user,
            PasswordResetTokens.Provider,
            PasswordResetTokens.BlockedUntilUtc);

        if (TryParseUtc(blockedUntilText, out DateTime blockedUntilUtc) && blockedUntilUtc > DateTime.UtcNow)
        {
            return Results.Json(
                new
                {
                    reset = false,
                    error = "Too many failed attempts.",
                    blockedUntilUtc
                },
                statusCode: StatusCodes.Status429TooManyRequests);
        }

        string? expectedCodeHash = await userManager.GetAuthenticationTokenAsync(
            user,
            PasswordResetTokens.Provider,
            PasswordResetTokens.CodeHash);

        string? expiryText = await userManager.GetAuthenticationTokenAsync(
            user,
            PasswordResetTokens.Provider,
            PasswordResetTokens.ExpiryUtc);

        if (string.IsNullOrWhiteSpace(expectedCodeHash) ||
            string.IsNullOrWhiteSpace(expiryText) ||
            !TryParseUtc(expiryText, out DateTime expiresAtUtc))
        {
            return Results.BadRequest(new { error = "Reset code is missing. Request a new code." });
        }

        if (DateTime.UtcNow > expiresAtUtc)
        {
            return Results.BadRequest(new { error = "Reset code has expired." });
        }

        string currentCodeHash = ComputeSha256(code);
        if (!FixedEquals(expectedCodeHash, currentCodeHash))
        {
            string? failedAttemptsText = await userManager.GetAuthenticationTokenAsync(
                user,
                PasswordResetTokens.Provider,
                PasswordResetTokens.FailedAttempts);

            int failedAttempts = int.TryParse(failedAttemptsText, NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsed)
                ? parsed
                : 0;

            failedAttempts++;

            int maxFailedAttempts = Math.Max(1, authSecurityOptions.EmailVerificationMaxFailedAttempts);
            if (failedAttempts >= maxFailedAttempts)
            {
                DateTime blockUntilUtc = DateTime.UtcNow.AddMinutes(Math.Max(1, authSecurityOptions.EmailVerificationBlockMinutes));

                await userManager.SetAuthenticationTokenAsync(
                    user,
                    PasswordResetTokens.Provider,
                    PasswordResetTokens.BlockedUntilUtc,
                    blockUntilUtc.ToString("O"));

                await userManager.SetAuthenticationTokenAsync(
                    user,
                    PasswordResetTokens.Provider,
                    PasswordResetTokens.FailedAttempts,
                    "0");

                return Results.Json(
                    new
                    {
                        reset = false,
                        error = "Too many failed attempts.",
                        blockedUntilUtc = blockUntilUtc
                    },
                    statusCode: StatusCodes.Status429TooManyRequests);
            }

            await userManager.SetAuthenticationTokenAsync(
                user,
                PasswordResetTokens.Provider,
                PasswordResetTokens.FailedAttempts,
                failedAttempts.ToString(CultureInfo.InvariantCulture));

            return Results.BadRequest(new { error = "Reset code is invalid." });
        }

        string resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
        IdentityResult resetResult = await userManager.ResetPasswordAsync(user, resetToken, request.NewPassword);
        if (!resetResult.Succeeded)
        {
            return Results.BadRequest(new
            {
                error = "Password reset failed.",
                details = resetResult.Errors.Select(x => x.Description).ToArray()
            });
        }

        await userManager.UpdateSecurityStampAsync(user);
        await userManager.RemoveAuthenticationTokenAsync(user, PasswordResetTokens.Provider, PasswordResetTokens.CodeHash);
        await userManager.RemoveAuthenticationTokenAsync(user, PasswordResetTokens.Provider, PasswordResetTokens.ExpiryUtc);
        await userManager.RemoveAuthenticationTokenAsync(user, PasswordResetTokens.Provider, PasswordResetTokens.LastSentAtUtc);
        await userManager.RemoveAuthenticationTokenAsync(user, PasswordResetTokens.Provider, PasswordResetTokens.FailedAttempts);
        await userManager.RemoveAuthenticationTokenAsync(user, PasswordResetTokens.Provider, PasswordResetTokens.BlockedUntilUtc);

        await auditTrailService.RecordAsync(
            new AuditRecordRequest(
                user.Id.ToString("N"),
                "auth.password-reset.success",
                "User",
                user.Id.ToString("N"),
                "{\"scope\":\"password-reset\"}"),
            cancellationToken);

        return Results.Ok(new { reset = true, userId = user.Id });
    }

    private static string ComputeSha256(string value)
    {
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(hash);
    }

    private static bool FixedEquals(string left, string right)
    {
        byte[] leftBytes = Encoding.UTF8.GetBytes(left);
        byte[] rightBytes = Encoding.UTF8.GetBytes(right);
        return CryptographicOperations.FixedTimeEquals(leftBytes, rightBytes);
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
