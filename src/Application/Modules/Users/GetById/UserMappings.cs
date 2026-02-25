using System.Linq.Expressions;
using Domain.Users;

namespace Application.Users.GetById;

internal static class UserMappings
{
    internal static readonly Expression<Func<User, UserResponse>> ToModel = user => new UserResponse
    {
        Id = user.Id,
        FirstName = user.FirstName,
        LastName = user.LastName,
        Email = user.Email ?? string.Empty,
        PhoneNumber = user.PhoneNumber,
        EmailConfirmed = user.EmailConfirmed,
        PhoneNumberConfirmed = user.PhoneNumberConfirmed,
        TwoFactorEnabled = user.TwoFactorEnabled,
        LockoutEnabled = user.LockoutEnabled,
        LockoutEndUtc = user.LockoutEndUtc,
        FailedLoginCount = user.FailedLoginCount,
        AuditCreatedBy = user.AuditCreatedBy,
        AuditCreatedAtUtc = user.AuditCreatedAtUtc,
        AuditUpdatedBy = user.AuditUpdatedBy,
        AuditUpdatedAtUtc = user.AuditUpdatedAtUtc
    };
}
