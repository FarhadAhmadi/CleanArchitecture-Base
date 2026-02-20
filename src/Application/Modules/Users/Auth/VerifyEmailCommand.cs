using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Users.Register;
using Domain.Users;
using Infrastructure.Auditing;
using Microsoft.AspNetCore.Identity;

namespace Application.Users.Auth;

public sealed record VerifyEmailCommand(string Email, string Code) : ICommand<IResult>;

internal sealed class VerifyEmailCommandHandler(
    UserManager<User> userManager,
    AuthSecurityOptions authSecurityOptions,
    IAuditTrailService auditTrailService) : ResultWrappingCommandHandler<VerifyEmailCommand>
{
    protected override async Task<IResult> HandleCore(VerifyEmailCommand command, CancellationToken cancellationToken)
    {
        string email = command.Email?.Trim() ?? string.Empty;
        string code = command.Code?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(code))
        {
            return Results.BadRequest(new { error = "Email and code are required." });
        }

        User? user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            return Results.NotFound(new { error = "User not found." });
        }

        if (user.EmailConfirmed)
        {
            return Results.Ok(new { verified = true, alreadyVerified = true });
        }

        string? blockedUntilText = await userManager.GetAuthenticationTokenAsync(user, EmailVerificationTokens.Provider, EmailVerificationTokens.BlockedUntilUtc);
        if (TryParseUtc(blockedUntilText, out DateTime blockedUntilUtc) && blockedUntilUtc > DateTime.UtcNow)
        {
            return Results.StatusCode(StatusCodes.Status429TooManyRequests);
        }

        string? expectedCodeHash = await userManager.GetAuthenticationTokenAsync(user, EmailVerificationTokens.Provider, EmailVerificationTokens.CodeHash);
        string? expiryText = await userManager.GetAuthenticationTokenAsync(user, EmailVerificationTokens.Provider, EmailVerificationTokens.ExpiryUtc);

        if (string.IsNullOrWhiteSpace(expectedCodeHash) || string.IsNullOrWhiteSpace(expiryText) ||
            !DateTime.TryParseExact(expiryText, "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTime expiresAtUtc))
        {
            return Results.BadRequest(new { error = "Verification code is missing. Please register again." });
        }

        if (DateTime.UtcNow > DateTime.SpecifyKind(expiresAtUtc, DateTimeKind.Utc))
        {
            return Results.BadRequest(new { error = "Verification code has expired." });
        }

        string currentCodeHash = ComputeSha256(code);
        if (!FixedEquals(expectedCodeHash, currentCodeHash))
        {
            string? failedAttemptsText = await userManager.GetAuthenticationTokenAsync(user, EmailVerificationTokens.Provider, EmailVerificationTokens.FailedAttempts);
            int failedAttempts = int.TryParse(failedAttemptsText, NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsed) ? parsed : 0;
            failedAttempts++;

            int maxFailedAttempts = Math.Max(1, authSecurityOptions.EmailVerificationMaxFailedAttempts);
            if (failedAttempts >= maxFailedAttempts)
            {
                DateTime blockUntilUtc = DateTime.UtcNow.AddMinutes(Math.Max(1, authSecurityOptions.EmailVerificationBlockMinutes));

                await userManager.SetAuthenticationTokenAsync(user, EmailVerificationTokens.Provider, EmailVerificationTokens.BlockedUntilUtc, blockUntilUtc.ToString("O"));
                await userManager.SetAuthenticationTokenAsync(user, EmailVerificationTokens.Provider, EmailVerificationTokens.FailedAttempts, "0");

                return Results.StatusCode(StatusCodes.Status429TooManyRequests);
            }

            await userManager.SetAuthenticationTokenAsync(user, EmailVerificationTokens.Provider, EmailVerificationTokens.FailedAttempts, failedAttempts.ToString(CultureInfo.InvariantCulture));
            return Results.BadRequest(new { error = "Verification code is invalid." });
        }

        user.EmailConfirmed = true;
        IdentityResult updateResult = await userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            return Results.Problem("Failed to confirm email.", statusCode: StatusCodes.Status500InternalServerError);
        }

        await userManager.RemoveAuthenticationTokenAsync(user, EmailVerificationTokens.Provider, EmailVerificationTokens.CodeHash);
        await userManager.RemoveAuthenticationTokenAsync(user, EmailVerificationTokens.Provider, EmailVerificationTokens.ExpiryUtc);
        await userManager.RemoveAuthenticationTokenAsync(user, EmailVerificationTokens.Provider, EmailVerificationTokens.LastSentAtUtc);
        await userManager.RemoveAuthenticationTokenAsync(user, EmailVerificationTokens.Provider, EmailVerificationTokens.FailedAttempts);
        await userManager.RemoveAuthenticationTokenAsync(user, EmailVerificationTokens.Provider, EmailVerificationTokens.BlockedUntilUtc);

        await auditTrailService.RecordAsync(
            new AuditRecordRequest(
                user.Id.ToString("N"),
                "auth.verify-email.success",
                "User",
                user.Id.ToString("N"),
                "{\"scope\":\"email-verification\"}"),
            cancellationToken);

        return Results.Ok(new { verified = true, userId = user.Id });
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
        if (DateTime.TryParseExact(value, "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTime parsed))
        {
            utcValue = DateTime.SpecifyKind(parsed, DateTimeKind.Utc);
            return true;
        }

        utcValue = default;
        return false;
    }
}
