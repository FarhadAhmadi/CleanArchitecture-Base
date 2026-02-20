using System.Security.Claims;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Authorization;
using Domain.Logging;
using Infrastructure.Auditing;
using Infrastructure.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel;
using Application.Shared;

namespace Application.Logging;

public sealed record GetLoggingAccessControlQuery : IQuery<IResult>;
internal sealed class GetLoggingAccessControlQueryHandler(IApplicationReadDbContext readContext) : ResultWrappingQueryHandler<GetLoggingAccessControlQuery>
{
    protected override async Task<IResult> HandleCore(GetLoggingAccessControlQuery query, CancellationToken cancellationToken)
    {
        List<string> permissions = await readContext.Permissions
            .Where(x => x.Code.StartsWith("logging.", StringComparison.OrdinalIgnoreCase))
            .Select(x => x.Code)
            .OrderBy(x => x)
            .ToListAsync(cancellationToken);

        var roles = await readContext.Roles
            .Select(role => new
            {
                role.Name,
                Permissions = (
                    from rp in readContext.RolePermissions
                    join p in readContext.Permissions on rp.PermissionId equals p.Id
                    where rp.RoleId == role.Id && p.Code.StartsWith("logging.")
                    select p.Code).ToList()
            })
            .ToListAsync(cancellationToken);

        return Results.Ok(new { permissions, roles });
    }
}





