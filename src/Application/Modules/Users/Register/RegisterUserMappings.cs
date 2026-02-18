using Domain.Users;

namespace Application.Users.Register;

internal static class RegisterUserMappings
{
    internal static User ToEntity(this RegisterUserCommand command, string passwordHash)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Email = command.Email,
            FirstName = command.FirstName,
            LastName = command.LastName,
            PasswordHash = passwordHash
        };
    }
}
