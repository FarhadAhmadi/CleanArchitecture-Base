using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Authorization;
using Domain.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Users.Register;

internal sealed class RegisterUserCommandHandler(
    IApplicationDbContext context,
    UserManager<User> userManager,
    RoleManager<Role> roleManager)
    : ICommandHandler<RegisterUserCommand, Guid>
{
    private const string DefaultUserRoleName = "user";

    public async Task<Result<Guid>> Handle(RegisterUserCommand command, CancellationToken cancellationToken)
    {
        if (await userManager.FindByEmailAsync(command.Email) is not null)
        {
            return Result.Failure<Guid>(UserErrors.EmailNotUnique);
        }

        User user = command.ToEntity();

        IdentityResult createResult = await userManager.CreateAsync(user, command.Password);

        if (!createResult.Succeeded)
        {
            string description = string.Join("; ", createResult.Errors.Select(e => e.Description));
            return Result.Failure<Guid>(Error.Problem("Users.CreateFailed", description));
        }

        Role? defaultRole = await roleManager.FindByNameAsync(DefaultUserRoleName)
            ?? await context.Roles.SingleOrDefaultAsync(r => r.Name == DefaultUserRoleName, cancellationToken);

        if (defaultRole is not null)
        {
            bool alreadyAssigned = await context.UserRoles.AnyAsync(
                x => x.UserId == user.Id && x.RoleId == defaultRole.Id,
                cancellationToken);

            if (!alreadyAssigned)
            {
                context.UserRoles.Add(new UserRole
                {
                    UserId = user.Id,
                    RoleId = defaultRole.Id
                });
            }
        }

        user.Raise(new UserRegisteredDomainEvent(user.Id));
        await context.SaveChangesAsync(cancellationToken);

        return user.Id;
    }
}
