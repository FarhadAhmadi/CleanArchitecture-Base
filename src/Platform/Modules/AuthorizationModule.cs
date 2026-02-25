using Infrastructure.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

internal static class AuthorizationModule
{
    internal static IServiceCollection AddAuthorizationModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        AuthorizationBootstrapOptions bootstrapOptions = configuration
            .GetSection(AuthorizationBootstrapOptions.SectionName)
            .Get<AuthorizationBootstrapOptions>() ?? new AuthorizationBootstrapOptions();

        services.AddSingleton(bootstrapOptions);
        services.AddScoped<AuthorizationSeeder>();

        services.AddAuthorization();
        services.AddScoped<PermissionProvider>();
        services.AddTransient<IAuthorizationHandler, PermissionAuthorizationHandler>();
        services.AddTransient<IAuthorizationPolicyProvider, PermissionAuthorizationPolicyProvider>();

        return services;
    }
}
