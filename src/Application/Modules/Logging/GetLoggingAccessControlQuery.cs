using System.Security.Claims;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Authorization;
using Domain.Logging;
using Application.Abstractions.Auditing;
using Application.Abstractions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel;
using Application.Shared;

namespace Application.Logging;

public sealed record GetLoggingAccessControlQuery : IQuery<IResult>;
internal sealed class GetLoggingAccessControlQueryHandler(ILoggingReadDbContext readContext) : ResultWrappingQueryHandler<GetLoggingAccessControlQuery>
{
    protected override async Task<IResult> HandleCore(GetLoggingAccessControlQuery query, CancellationToken cancellationToken)
    {
        var permissions = (await readContext.Permissions
                .Select(x => x.Code)
                .ToListAsync(cancellationToken))
            .Where(x => x.StartsWith("logging.", StringComparison.OrdinalIgnoreCase))
            .OrderBy(x => x)
            .ToList();

        List<RolePermissionProjection> rolePermissions = await (
                from rp in readContext.RolePermissions
                join p in readContext.Permissions on rp.PermissionId equals p.Id
                select new RolePermissionProjection(rp.RoleId, p.Code))
            .ToListAsync(cancellationToken);

        var roles = (await readContext.Roles
                .Select(role => new { role.Id, role.Name })
                .ToListAsync(cancellationToken))
            .Select(role => new
            {
                role.Name,
                Permissions = rolePermissions
                    .Where(x => x.RoleId == role.Id && x.PermissionCode.StartsWith("logging.", StringComparison.OrdinalIgnoreCase))
                    .Select(x => x.PermissionCode)
                    .Distinct()
                    .OrderBy(x => x)
                    .ToList()
            })
            .ToList();

        return Results.Ok(new { permissions, roles });
    }
}

internal sealed record RolePermissionProjection(Guid RoleId, string PermissionCode);


