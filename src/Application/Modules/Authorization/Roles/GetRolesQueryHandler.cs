using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Authorization.Roles;

internal sealed class GetRolesQueryHandler(IApplicationReadDbContext context)
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
                Name = x.Name ?? string.Empty
            })
            .ToListAsync(cancellationToken);

        return roles;
    }
}
