using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Authorization.GetAccessControl;

internal sealed class GetAccessControlQueryHandler(IApplicationDbContext context)
    : IQueryHandler<GetAccessControlQuery, AccessControlResponse>
{
    public async Task<Result<AccessControlResponse>> Handle(GetAccessControlQuery query, CancellationToken cancellationToken)
    {
        List<PermissionResponse> permissions = await context.Permissions
            .OrderBy(x => x.Code)
            .Select(x => new PermissionResponse
            {
                Id = x.Id,
                Code = x.Code,
                Description = x.Description
            })
            .ToListAsync(cancellationToken);

        List<(Guid RoleId, string RoleName)> rolesRaw = await context.Roles
            .OrderBy(x => x.Name)
            .Select(x => new ValueTuple<Guid, string>(x.Id, x.Name))
            .ToListAsync(cancellationToken);

        List<(Guid RoleId, string PermissionCode)> rolePermissionsRaw = await (
            from rolePermission in context.RolePermissions
            join permission in context.Permissions on rolePermission.PermissionId equals permission.Id
            select new ValueTuple<Guid, string>(rolePermission.RoleId, permission.Code))
            .ToListAsync(cancellationToken);

        var roles = rolesRaw
            .Select(role => new RoleResponse
            {
                Id = role.RoleId,
                Name = role.RoleName,
                Permissions = [.. rolePermissionsRaw
                    .Where(x => x.RoleId == role.RoleId)
                    .Select(x => x.PermissionCode)]
            })
            .ToList();

        return new AccessControlResponse
        {
            Roles = roles,
            Permissions = permissions
        };
    }
}
