using Application.Abstractions.Messaging;
using Application.Abstractions.Data;
using Domain.Users;
using Microsoft.AspNetCore.Identity;
using SharedKernel;

namespace Application.Users.Management;

internal sealed class CreateUserCommandHandler(
    UserManager<User> userManager,
    IUsersWriteDbContext context)
    : ICommandHandler<CreateUserCommand, UserAdminResponse>
{
    public async Task<Result<UserAdminResponse>> Handle(CreateUserCommand command, CancellationToken cancellationToken)
    {
        User? existing = await userManager.FindByEmailAsync(command.Email);
        if (existing is not null)
        {
            return Result.Failure<UserAdminResponse>(UserErrors.EmailNotUnique);
        }

        User user = new()
        {
            Email = command.Email,
            UserName = command.Email,
            FirstName = command.FirstName,
            LastName = command.LastName,
            PhoneNumber = string.IsNullOrWhiteSpace(command.PhoneNumber) ? null : command.PhoneNumber.Trim(),
            EmailConfirmed = command.EmailConfirmed ?? false,
            PhoneNumberConfirmed = command.PhoneNumberConfirmed ?? false,
            TwoFactorEnabled = command.TwoFactorEnabled ?? false,
            LockoutEnabled = command.LockoutEnabled ?? true,
            FailedLoginCount = Math.Max(command.FailedLoginCount ?? 0, 0),
            LockoutEndUtc = command.LockoutEndUtc
        };

        IdentityResult createResult = await userManager.CreateAsync(user, command.Password);
        if (!createResult.Succeeded)
        {
            string description = string.Join("; ", createResult.Errors.Select(e => e.Description));
            return Result.Failure<UserAdminResponse>(Error.Problem("Users.CreateFailed", description));
        }

        if (!string.IsNullOrWhiteSpace(user.PasswordHash))
        {
            context.UserPasswordHistories.Add(new UserPasswordHistory
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                PasswordHash = user.PasswordHash,
                CreatedAtUtc = DateTime.UtcNow
            });

            await context.SaveChangesAsync(cancellationToken);
        }

        return new UserAdminResponse
        {
            Id = user.Id,
            Email = user.Email ?? command.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            EmailConfirmed = user.EmailConfirmed,
            PhoneNumberConfirmed = user.PhoneNumberConfirmed,
            TwoFactorEnabled = user.TwoFactorEnabled,
            LockoutEnabled = user.LockoutEnabled,
            LockoutEndUtc = user.LockoutEndUtc,
            FailedLoginCount = user.FailedLoginCount,
            AuditCreatedBy = user.AuditCreatedBy,
            AuditCreatedAtUtc = user.AuditCreatedAtUtc,
            AuditUpdatedBy = user.AuditUpdatedBy,
            AuditUpdatedAtUtc = user.AuditUpdatedAtUtc
        };
    }
}
