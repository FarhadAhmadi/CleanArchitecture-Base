using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Authorization.Roles;

internal sealed class GetRoleByIdQueryHandler(IApplicationReadDbContext context)
    : IQueryHandler<GetRoleByIdQuery, RoleCrudResponse>
{
    public async Task<Result<RoleCrudResponse>> Handle(
        GetRoleByIdQuery query,
        CancellationToken cancellationToken)
    {
        RoleCrudResponse? role = await context.Roles
            .Where(x => x.Id == query.RoleId)
            .Select(x => new RoleCrudResponse
            {
                Id = x.Id,
                Name = x.Name ?? string.Empty
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (role is null)
        {
            return Result.Failure<RoleCrudResponse>(
                Error.NotFound("Roles.NotFound", $"Role '{query.RoleId}' was not found."));
        }

        return role;
    }
}
