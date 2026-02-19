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

        services.AddSingleton(jwtOptions);

        services.AddIdentityCore<User>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
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
}
