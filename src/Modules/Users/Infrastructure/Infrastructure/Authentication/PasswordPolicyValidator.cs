using Application.Abstractions.Data;
using Application.Abstractions.Security;
using Domain.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Authentication;

internal sealed class PasswordPolicyValidator(
    AuthSecurityOptions authSecurityOptions,
    IUsersWriteDbContext dbContext) : IPasswordValidator<User>
{
    private static readonly string[] DefaultBreachedPasswords =
    [
        "password",
        "password123",
        "123456",
        "12345678",
        "qwerty",
        "qwerty123",
        "letmein",
        "admin",
        "welcome",
        "iloveyou",
        "passw0rd"
    ];

    private readonly PasswordHasher<User> _passwordHasher = new();

    public async Task<IdentityResult> ValidateAsync(
        UserManager<User> manager,
        User user,
        string? password)
    {
        List<IdentityError> errors = [];
        string candidate = password?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(candidate))
        {
            errors.Add(new IdentityError
            {
                Code = "PasswordRequired",
                Description = "Password is required."
            });
            return IdentityResult.Failed([.. errors]);
        }

        if (candidate.Length < Math.Max(8, authSecurityOptions.PasswordMinLength))
        {
            errors.Add(new IdentityError
            {
                Code = "PasswordTooShort",
                Description = $"Password must be at least {Math.Max(8, authSecurityOptions.PasswordMinLength)} characters."
            });
        }

        if (authSecurityOptions.PasswordRequireDigit && !candidate.Any(char.IsDigit))
        {
            errors.Add(new IdentityError { Code = "PasswordRequiresDigit", Description = "Password must include at least one digit." });
        }

        if (authSecurityOptions.PasswordRequireLowercase && !candidate.Any(char.IsLower))
        {
            errors.Add(new IdentityError { Code = "PasswordRequiresLower", Description = "Password must include at least one lowercase letter." });
        }

        if (authSecurityOptions.PasswordRequireUppercase && !candidate.Any(char.IsUpper))
        {
            errors.Add(new IdentityError { Code = "PasswordRequiresUpper", Description = "Password must include at least one uppercase letter." });
        }

        if (authSecurityOptions.PasswordRequireNonAlphanumeric && candidate.All(char.IsLetterOrDigit))
        {
            errors.Add(new IdentityError { Code = "PasswordRequiresSpecial", Description = "Password must include at least one special character." });
        }

        int requiredUniqueChars = Math.Max(1, authSecurityOptions.PasswordRequiredUniqueChars);
        if (candidate.Distinct().Count() < requiredUniqueChars)
        {
            errors.Add(new IdentityError
            {
                Code = "PasswordRequiresUniqueChars",
                Description = $"Password must include at least {requiredUniqueChars} unique characters."
            });
        }

        if (LooksBreachedOrCommon(candidate, user.Email, authSecurityOptions.BreachedPasswordDenyList))
        {
            errors.Add(new IdentityError
            {
                Code = "PasswordBreachedOrCommon",
                Description = "Password is too common or appears in a breached pattern."
            });
        }

        if (!string.IsNullOrWhiteSpace(user.PasswordHash) &&
            _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, candidate) is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded)
        {
            errors.Add(new IdentityError
            {
                Code = "PasswordReuseCurrent",
                Description = "New password must be different from the current password."
            });
        }

        int historyLimit = Math.Max(1, authSecurityOptions.PasswordHistoryLimit);
        List<string> passwordHistory = await dbContext.UserPasswordHistories
            .Where(x => x.UserId == user.Id)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => x.PasswordHash)
            .Take(historyLimit)
            .ToListAsync();

        if (passwordHistory.Any(hash =>
                _passwordHasher.VerifyHashedPassword(user, hash, candidate) is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded))
        {
            errors.Add(new IdentityError
            {
                Code = "PasswordReuseHistory",
                Description = $"Password must not match the last {historyLimit} passwords."
            });
        }

        return errors.Count == 0
            ? IdentityResult.Success
            : IdentityResult.Failed([.. errors]);
    }

    private static bool LooksBreachedOrCommon(string password, string? email, IReadOnlyCollection<string> configuredDenyList)
    {
        string normalized = password.Trim().ToUpperInvariant();

        if (DefaultBreachedPasswords.Contains(normalized, StringComparer.Ordinal))
        {
            return true;
        }

        if (configuredDenyList.Any(item => string.Equals(item?.Trim(), normalized, StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }

        if (!string.IsNullOrWhiteSpace(email))
        {
            string localPart = email.Split('@')[0].Trim().ToUpperInvariant();
            if (!string.IsNullOrWhiteSpace(localPart) && normalized.Contains(localPart, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }
}
