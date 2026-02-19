using Domain.Users;

namespace Application.Users.Register;

internal static class RegisterUserMappings
{
    internal static User ToEntity(this RegisterUserCommand command)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Email = command.Email,
            UserName = command.Email,
            NormalizedEmail = command.Email.ToUpperInvariant(),
            NormalizedUserName = command.Email.ToUpperInvariant(),
            FirstName = command.FirstName,
            LastName = command.LastName,
            EmailConfirmed = true,
            LockoutEnabled = true,
            SecurityStamp = Guid.NewGuid().ToString("N")
        };
    }
}
