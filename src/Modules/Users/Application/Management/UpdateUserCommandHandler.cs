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
            LastName = user.LastName
        };
    }
}
