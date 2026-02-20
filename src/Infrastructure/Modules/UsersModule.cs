using Application.Abstractions.Authentication;
using Application.Abstractions.Users;
using Application.Abstractions.Security;
using Infrastructure.Authentication;
using Infrastructure.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

internal static class UsersModule
{
    internal static IServiceCollection AddUsersModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        RefreshTokenOptions refreshTokenOptions = configuration
            .GetSection(RefreshTokenOptions.SectionName)
            .Get<RefreshTokenOptions>() ?? new RefreshTokenOptions();

        AuthSecurityOptions authSecurityOptions = configuration
            .GetSection(AuthSecurityOptions.SectionName)
            .Get<AuthSecurityOptions>() ?? new AuthSecurityOptions();

        ValidateRefreshTokenOptions(refreshTokenOptions);

        services.AddSingleton(refreshTokenOptions);
        services.AddSingleton(authSecurityOptions);

        services.AddHttpContextAccessor();
        services.AddScoped<IUserContext, UserContext>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<ITokenProvider, TokenProvider>();
        services.AddSingleton<IRefreshTokenProvider, RefreshTokenProvider>();
        services.AddSingleton<ITokenLifetimeProvider, TokenLifetimeProvider>();
        services.AddSingleton<ISecurityEventLogger, SecurityEventLogger>();
        services.AddScoped<IUserRegistrationVerificationService, UserRegistrationVerificationService>();
        services.AddScoped<IUserPasswordResetService, UserPasswordResetService>();

        return services;
    }

    private static void ValidateRefreshTokenOptions(RefreshTokenOptions options)
    {
        if (options.ExpirationInDays is < 1 or > 180)
        {
            throw new InvalidOperationException("RefreshToken:ExpirationInDays must be between 1 and 180.");
        }
    }
}
