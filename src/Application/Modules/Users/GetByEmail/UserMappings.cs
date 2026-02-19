using System.Linq.Expressions;
using Domain.Users;

namespace Application.Users.GetByEmail;

internal static class UserMappings
{
    internal static readonly Expression<Func<User, UserResponse>> ToModel = user => new UserResponse
    {
        Id = user.Id,
        FirstName = user.FirstName,
        LastName = user.LastName,
        Email = user.Email ?? string.Empty
    };
}
