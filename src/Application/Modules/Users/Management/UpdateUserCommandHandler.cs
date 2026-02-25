using Application.Abstractions.Messaging;
using Domain.Users;
using Microsoft.AspNetCore.Identity;
using SharedKernel;

namespace Application.Users.Management;

internal sealed class UpdateUserCommandHandler(UserManager<User> userManager)
    : ICommandHandler<UpdateUserCommand, UserAdminResponse>
{
    public async Task<Result<UserAdminResponse>> Handle(UpdateUserCommand command, CancellationToken cancellationToken)
    {
        User? user = await userManager.FindByIdAsync(command.UserId.ToString());
        if (user is null)
        {
            return Result.Failure<UserAdminResponse>(UserErrors.NotFound(command.UserId));
        }

        User? existingByEmail = await userManager.FindByEmailAsync(command.Email);
        if (existingByEmail is not null && existingByEmail.Id != user.Id)
        {
            return Result.Failure<UserAdminResponse>(UserErrors.EmailNotUnique);
        }

        user.Email = command.Email;
        user.UserName = command.Email;
        user.FirstName = command.FirstName;
        user.LastName = command.LastName;
        if (command.PhoneNumber is not null)
        {
            user.PhoneNumber = string.IsNullOrWhiteSpace(command.PhoneNumber) ? null : command.PhoneNumber.Trim();
        }

        if (command.EmailConfirmed.HasValue)
        {
            user.EmailConfirmed = command.EmailConfirmed.Value;
        }

        if (command.PhoneNumberConfirmed.HasValue)
        {
            user.PhoneNumberConfirmed = command.PhoneNumberConfirmed.Value;
        }

        if (command.TwoFactorEnabled.HasValue)
        {
            user.TwoFactorEnabled = command.TwoFactorEnabled.Value;
        }

        if (command.LockoutEnabled.HasValue)
        {
            user.LockoutEnabled = command.LockoutEnabled.Value;
        }

        if (command.ClearLockoutEnd == true)
        {
            user.LockoutEndUtc = null;
        }
        else if (command.LockoutEndUtc.HasValue)
        {
            user.LockoutEndUtc = command.LockoutEndUtc.Value;
        }

        if (command.FailedLoginCount.HasValue)
        {
            user.FailedLoginCount = Math.Max(command.FailedLoginCount.Value, 0);
        }

        IdentityResult updateResult = await userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            string description = string.Join("; ", updateResult.Errors.Select(e => e.Description));
            return Result.Failure<UserAdminResponse>(Error.Problem("Users.UpdateFailed", description));
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
