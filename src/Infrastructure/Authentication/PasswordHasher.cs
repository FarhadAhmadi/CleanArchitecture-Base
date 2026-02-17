using Application.Abstractions.Authentication;
using Domain.Users;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Authentication;

internal sealed class PasswordHasher : IPasswordHasher
{
    private readonly Microsoft.AspNetCore.Identity.PasswordHasher<User> _passwordHasher = new();
    private static readonly User HashUser = new()
    {
        Id = Guid.Empty,
        Email = "password-hash@system.local",
        FirstName = "System",
        LastName = "Hasher",
        PasswordHash = string.Empty
    };

    public string Hash(string password)
    {
        return _passwordHasher.HashPassword(HashUser, password);
    }

    public bool Verify(string password, string passwordHash)
    {
        PasswordVerificationResult result = _passwordHasher.VerifyHashedPassword(HashUser, passwordHash, password);
        return result is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
    }
}
