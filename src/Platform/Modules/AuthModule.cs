using System.Security.Claims;
using System.Text;
using Domain.Authorization;
using Domain.Users;
using Infrastructure.Database;
using Application.Abstractions.Security;
using Infrastructure.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure;

internal static class AuthModule
{
    internal static IServiceCollection AddAuthModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        AuthSecurityOptions authSecurityOptions = configuration
            .GetSection(AuthSecurityOptions.SectionName)
            .Get<AuthSecurityOptions>() ?? new AuthSecurityOptions();

        JwtOptions jwtOptions = configuration
            .GetSection(JwtOptions.SectionName)
            .Get<JwtOptions>() ?? new JwtOptions();

        if (string.IsNullOrWhiteSpace(jwtOptions.Secret) &&
            string.Equals(configuration["ASPNETCORE_ENVIRONMENT"], "Testing", StringComparison.OrdinalIgnoreCase))
        {
            jwtOptions.Secret = "test-super-duper-secret-value-with-32-chars";
            jwtOptions.Issuer = "test-issuer";
            jwtOptions.Audience = "test-audience";
            jwtOptions.ExpirationInMinutes = 60;
        }

        ValidateJwtOptions(jwtOptions);
        ValidateAuthSecurityOptions(authSecurityOptions);

        services.AddSingleton(jwtOptions);
        services.AddScoped<IPasswordValidator<User>, PasswordPolicyValidator>();

        services.AddIdentityCore<User>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = Math.Max(8, authSecurityOptions.PasswordMinLength);
                options.Password.RequireDigit = authSecurityOptions.PasswordRequireDigit;
                options.Password.RequireLowercase = authSecurityOptions.PasswordRequireLowercase;
                options.Password.RequireUppercase = authSecurityOptions.PasswordRequireUppercase;
                options.Password.RequireNonAlphanumeric = authSecurityOptions.PasswordRequireNonAlphanumeric;
                options.Password.RequiredUniqueChars = Math.Max(1, authSecurityOptions.PasswordRequiredUniqueChars);
                options.Lockout.MaxFailedAccessAttempts = Math.Max(1, authSecurityOptions.MaxFailedLoginAttempts);
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(Math.Max(1, authSecurityOptions.LockoutMinutes));
            })
            .AddRoles<Role>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddSignInManager();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(o =>
            {
                bool isDevelopmentOrTesting =
                    string.Equals(configuration["ASPNETCORE_ENVIRONMENT"], "Development", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(configuration["ASPNETCORE_ENVIRONMENT"], "Testing", StringComparison.OrdinalIgnoreCase);

                List<SecurityKey> signingKeys =
                [
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret))
                ];

                foreach (string previousSecret in jwtOptions.PreviousSecrets.Where(x => !string.IsNullOrWhiteSpace(x)))
                {
                    signingKeys.Add(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(previousSecret)));
                }

                o.RequireHttpsMetadata = !isDevelopmentOrTesting;
                o.IncludeErrorDetails = false;
                o.SaveToken = false;
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKeys = signingKeys,
                    ValidateIssuer = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtOptions.Audience,
                    ValidateLifetime = true,
                    NameClaimType = ClaimTypes.NameIdentifier,
                    ClockSkew = TimeSpan.Zero
                };
                o.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        ISecurityEventLogger securityLogger = context.HttpContext.RequestServices
                            .GetRequiredService<ISecurityEventLogger>();
                        securityLogger.AuthenticationFailed(
                            "JwtAuthenticationFailed",
                            context.Principal?.Identity?.Name,
                            context.HttpContext.Connection.RemoteIpAddress?.ToString(),
                            context.HttpContext.TraceIdentifier);
                        return Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        context.HandleResponse();
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;

                        ISecurityEventLogger securityLogger = context.HttpContext.RequestServices
                            .GetRequiredService<ISecurityEventLogger>();
                        securityLogger.AuthenticationFailed(
                            $"JwtChallenge:{context.Error}",
                            context.HttpContext.User.Identity?.Name,
                            context.HttpContext.Connection.RemoteIpAddress?.ToString(),
                            context.HttpContext.TraceIdentifier);
                        return Task.CompletedTask;
                    }
                };
            });

        return services;
    }

    private static void ValidateJwtOptions(JwtOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.Secret) || options.Secret.Length < 32)
        {
            throw new InvalidOperationException("Jwt:Secret must be at least 32 characters.");
        }

        foreach (string previousSecret in options.PreviousSecrets.Where(x => !string.IsNullOrWhiteSpace(x)))
        {
            if (previousSecret.Length < 32)
            {
                throw new InvalidOperationException("Jwt:PreviousSecrets items must be at least 32 characters.");
            }
        }

        if (string.IsNullOrWhiteSpace(options.Issuer))
        {
            throw new InvalidOperationException("Jwt:Issuer is required.");
        }

        if (string.IsNullOrWhiteSpace(options.Audience))
        {
            throw new InvalidOperationException("Jwt:Audience is required.");
        }

        if (options.ExpirationInMinutes <= 0)
        {
            throw new InvalidOperationException("Jwt:ExpirationInMinutes must be greater than zero.");
        }
    }

    private static void ValidateAuthSecurityOptions(AuthSecurityOptions options)
    {
        if (options.PasswordMinLength < 8 || options.PasswordMinLength > 128)
        {
            throw new InvalidOperationException("AuthSecurity:PasswordMinLength must be between 8 and 128.");
        }

        if (options.PasswordRequiredUniqueChars < 1 || options.PasswordRequiredUniqueChars > 32)
        {
            throw new InvalidOperationException("AuthSecurity:PasswordRequiredUniqueChars must be between 1 and 32.");
        }

        if (options.PasswordHistoryLimit < 1 || options.PasswordHistoryLimit > 24)
        {
            throw new InvalidOperationException("AuthSecurity:PasswordHistoryLimit must be between 1 and 24.");
        }
    }
}
