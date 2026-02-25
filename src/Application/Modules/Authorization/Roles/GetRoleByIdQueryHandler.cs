using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Authorization.Roles;

internal sealed class GetRoleByIdQueryHandler(IAuthorizationReadDbContext context)
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
                Name = x.Name ?? string.Empty,
                NormalizedName = x.NormalizedName ?? string.Empty,
                PermissionCount = context.RolePermissions.Count(rp => rp.RoleId == x.Id),
                UserCount = context.UserRoles.Count(ur => ur.RoleId == x.Id),
                IsSystemRole = (x.NormalizedName ?? string.Empty) == "ADMIN" || (x.NormalizedName ?? string.Empty) == "USER",
                AuditCreatedBy = x.AuditCreatedBy,
                AuditCreatedAtUtc = x.AuditCreatedAtUtc,
                AuditUpdatedBy = x.AuditUpdatedBy,
                AuditUpdatedAtUtc = x.AuditUpdatedAtUtc
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


