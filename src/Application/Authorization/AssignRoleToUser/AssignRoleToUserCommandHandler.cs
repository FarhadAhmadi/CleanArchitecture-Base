using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Authorization;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Authorization.AssignRoleToUser;

internal sealed class AssignRoleToUserCommandHandler(IApplicationDbContext context)
    : ICommandHandler<AssignRoleToUserCommand>
{
    public async Task<Result> Handle(AssignRoleToUserCommand command, CancellationToken cancellationToken)
    {
        bool userExists = await context.Users
            .AnyAsync(x => x.Id == command.UserId, cancellationToken);

        if (!userExists)
        {
            return Result.Failure(UserErrors.NotFound(command.UserId));
        }

        Role? role = await context.Roles
            .SingleOrDefaultAsync(x => x.Name == command.RoleName, cancellationToken);

        if (role is null)
        {
            return Result.Failure(Error.NotFound("Roles.NotFound", $"Role '{command.RoleName}' was not found."));
        }

        bool alreadyAssigned = await context.UserRoles.AnyAsync(
            x => x.UserId == command.UserId && x.RoleId == role.Id,
            cancellationToken);

        if (alreadyAssigned)
        {
            return Result.Success();
        }

        context.UserRoles.Add(new UserRole
        {
            UserId = command.UserId,
            RoleId = role.Id
        });

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
