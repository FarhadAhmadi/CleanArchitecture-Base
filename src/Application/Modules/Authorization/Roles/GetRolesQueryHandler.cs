using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Authorization.Roles;

internal sealed class GetRolesQueryHandler(IAuthorizationReadDbContext context)
    : IQueryHandler<GetRolesQuery, List<RoleCrudResponse>>
{
    public async Task<Result<List<RoleCrudResponse>>> Handle(
        GetRolesQuery query,
        CancellationToken cancellationToken)
    {
        List<RoleCrudResponse> roles = await context.Roles
            .ApplyContainsSearch(query.Search, x => x.Name)
            .OrderBy(x => x.Name)
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
            .ToListAsync(cancellationToken);

        return roles;
    }
}


