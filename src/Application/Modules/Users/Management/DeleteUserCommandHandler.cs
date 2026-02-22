using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Domain.Users;
using Microsoft.AspNetCore.Identity;
using SharedKernel;

namespace Application.Users.Management;

internal sealed class DeleteUserCommandHandler(
    UserManager<User> userManager,
    IUserContext userContext)
    : ICommandHandler<DeleteUserCommand>
{
    public async Task<Result> Handle(DeleteUserCommand command, CancellationToken cancellationToken)
    {
        if (command.UserId == userContext.UserId)
        {
            return Result.Failure(Error.Conflict("Users.DeleteSelfForbidden", "Cannot delete current user."));
        }

        User? user = await userManager.FindByIdAsync(command.UserId.ToString());
        if (user is null)
        {
            return Result.Failure(UserErrors.NotFound(command.UserId));
        }

        IdentityResult deleteResult = await userManager.DeleteAsync(user);
        if (!deleteResult.Succeeded)
        {
            string description = string.Join("; ", deleteResult.Errors.Select(e => e.Description));
            return Result.Failure(Error.Problem("Users.DeleteFailed", description));
        }

        return Result.Success();
    }
}
