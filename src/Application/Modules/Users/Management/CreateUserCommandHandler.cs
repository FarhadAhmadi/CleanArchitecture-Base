using Application.Abstractions.Messaging;
using Domain.Users;
using Microsoft.AspNetCore.Identity;
using SharedKernel;

namespace Application.Users.Management;

internal sealed class CreateUserCommandHandler(UserManager<User> userManager)
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
            LastName = command.LastName
        };

        IdentityResult createResult = await userManager.CreateAsync(user, command.Password);
        if (!createResult.Succeeded)
        {
            string description = string.Join("; ", createResult.Errors.Select(e => e.Description));
            return Result.Failure<UserAdminResponse>(Error.Problem("Users.CreateFailed", description));
        }

        return new UserAdminResponse
        {
            Id = user.Id,
            Email = user.Email ?? command.Email,
            FirstName = user.FirstName,
            LastName = user.LastName
        };
    }
}
