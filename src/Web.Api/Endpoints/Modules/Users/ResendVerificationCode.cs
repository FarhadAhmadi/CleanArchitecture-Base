using Application.Abstractions.Users;
using Domain.Users;
using Infrastructure.Auditing;
using Microsoft.AspNetCore.Identity;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Users;

internal sealed class ResendVerificationCode : IEndpoint
{
    public sealed record Request(string Email);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("users/resend-verification-code", async (
            Request request,
            UserManager<User> userManager,
            IUserRegistrationVerificationService verificationService,
            IAuditTrailService auditTrailService,
            CancellationToken cancellationToken) =>
        {
            string email = InputSanitizer.SanitizeEmail(request.Email) ?? string.Empty;
            if (string.IsNullOrWhiteSpace(email))
            {
                return Results.BadRequest(new { error = "Email is required." });
            }

            User? user = await userManager.FindByEmailAsync(email);
            if (user is null)
            {
                return Results.NotFound(new { error = "User not found." });
            }

            ResendVerificationResult resendResult = await verificationService.TryResendVerificationAsync(user.Id, cancellationToken);

            await auditTrailService.RecordAsync(
                new AuditRecordRequest(
                    user.Id.ToString("N"),
                    resendResult.Sent ? "auth.verify-email.resend.success" : "auth.verify-email.resend.blocked",
                    "User",
                    user.Id.ToString("N"),
                    "{\"scope\":\"email-verification-resend\"}"),
                cancellationToken);

            if (!resendResult.Sent)
            {
                if (string.Equals(resendResult.Error, "AlreadyVerified", StringComparison.Ordinal))
                {
                    return Results.Ok(new { sent = false, alreadyVerified = true });
                }

                if (string.Equals(resendResult.Error, "CooldownActive", StringComparison.Ordinal))
                {
                    return Results.Json(
                        new
                        {
                            sent = false,
                            error = "Cooldown is active.",
                            resendAvailableAtUtc = resendResult.CooldownEndsAtUtc
                        },
                        statusCode: StatusCodes.Status429TooManyRequests);
                }

                return Results.BadRequest(new { sent = false, error = "Verification code cannot be resent now." });
            }

            return Results.Ok(new
            {
                sent = true,
                verificationExpiresAtUtc = resendResult.VerificationExpiresAtUtc,
                resendAvailableAtUtc = resendResult.CooldownEndsAtUtc
            });
        })
        .WithTags(Tags.Users);
    }
}
