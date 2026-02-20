using Application.Abstractions.Messaging;
using Application.Abstractions.Users;
using Infrastructure.Auditing;

namespace Application.Users.Auth;

public sealed record RequestPasswordResetCommand(string Email) : ICommand<IResult>;

internal sealed class RequestPasswordResetCommandHandler(
    IUserPasswordResetService passwordResetService,
    IAuditTrailService auditTrailService) : ResultWrappingCommandHandler<RequestPasswordResetCommand>
{
    protected override async Task<IResult> HandleCore(RequestPasswordResetCommand command, CancellationToken cancellationToken)
    {
        string email = command.Email?.Trim() ?? string.Empty;
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
}
