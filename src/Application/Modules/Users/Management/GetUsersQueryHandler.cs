using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Users.Management;

internal sealed class GetUsersQueryHandler(IApplicationReadDbContext context)
    : IQueryHandler<GetUsersQuery, List<UserAdminResponse>>
{
    public async Task<Result<List<UserAdminResponse>>> Handle(
        GetUsersQuery query,
        CancellationToken cancellationToken)
    {
        IQueryable<User> usersQuery = context.Users
            .ApplyContainsSearch(query.Search, x => x.Email, x => x.FirstName, x => x.LastName);

        List<UserAdminResponse> users = await usersQuery
            .OrderBy(x => x.Email)
            .Select(x => new UserAdminResponse
            {
                Id = x.Id,
                Email = x.Email ?? string.Empty,
                FirstName = x.FirstName,
                LastName = x.LastName
            })
            .ToListAsync(cancellationToken);

        return users;
    }
}
