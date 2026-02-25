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
                LastName = x.LastName,
                PhoneNumber = x.PhoneNumber,
                EmailConfirmed = x.EmailConfirmed,
                PhoneNumberConfirmed = x.PhoneNumberConfirmed,
                TwoFactorEnabled = x.TwoFactorEnabled,
                LockoutEnabled = x.LockoutEnabled,
                LockoutEndUtc = x.LockoutEndUtc,
                FailedLoginCount = x.FailedLoginCount,
                AuditCreatedBy = x.AuditCreatedBy,
                AuditCreatedAtUtc = x.AuditCreatedAtUtc,
                AuditUpdatedBy = x.AuditUpdatedBy,
                AuditUpdatedAtUtc = x.AuditUpdatedAtUtc
            })
            .ToListAsync(cancellationToken);

        if (users.Count == 0)
        {
            return users;
        }

        HashSet<Guid> userIds = [.. users.Select(x => x.Id)];
        List<(Guid UserId, string RoleName)> userRoles = await (
            from userRole in context.UserRoles
            join role in context.Roles on userRole.RoleId equals role.Id
            where userIds.Contains(userRole.UserId)
            select new ValueTuple<Guid, string>(userRole.UserId, role.Name ?? string.Empty))
            .ToListAsync(cancellationToken);

        var rolesByUser = userRoles
            .GroupBy(x => x.UserId)
            .ToDictionary(
                group => group.Key,
                group => group
                    .Select(x => x.RoleName)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
                    .ToList());

        users = [.. users.Select(user =>
            user with { Roles = rolesByUser.GetValueOrDefault(user.Id, []) })];

        return users;
    }
}


