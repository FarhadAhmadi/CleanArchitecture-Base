using System.Security.Claims;
using System.Text;
using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Infrastructure.Authentication;
using Infrastructure.Authorization;
using Infrastructure.Database;
using Infrastructure.DomainEvents;
using Infrastructure.Logging;
using Infrastructure.Time;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using SharedKernel;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration) =>
        services
            .AddServices()
            .AddDatabase(configuration)
            .AddHealthChecks(configuration)
            .AddLoggingPlatform(configuration)
            .AddAuthenticationInternal(configuration)
            .AddAuthorizationInternal(configuration);

    private static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.AddTransient<IDomainEventsDispatcher, DomainEventsDispatcher>();
        return services;
    }

    private static IServiceCollection AddDatabase(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        string? connectionString = configuration.GetConnectionString("Database");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString, sqlOptions =>
                sqlOptions.MigrationsHistoryTable(
                    HistoryRepository.DefaultTableName,
                    Schemas.Default)));

        services.AddScoped<IApplicationDbContext>(sp =>
            sp.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<IApplicationReadDbContext, ApplicationReadDbContext>();

        return services;
    }

    private static IServiceCollection AddHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddHealthChecks()
            .AddSqlServer(configuration.GetConnectionString("Database")!);

        return services;
    }

    private static IServiceCollection AddLoggingPlatform(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        LoggingOptions options = configuration
            .GetSection(LoggingOptions.SectionName)
            .Get<LoggingOptions>() ?? new LoggingOptions();

        services.AddSingleton(options);
        services.AddSingleton<ILogSanitizer, LogSanitizer>();
        services.AddSingleton<ILogIntegrityService, LogIntegrityService>();
        services.AddSingleton<ILogIngestionQueue, LogIngestionQueue>();
        services.AddSingleton<IAlertDispatchQueue, AlertDispatchQueue>();
        services.AddScoped<ILogIngestionService, LogIngestionService>();
        services.AddSingleton<LoggingHealthService>();
        services.AddHostedService<LogRetryWorker>();
        services.AddHostedService<AlertDispatchWorker>();

        return services;
    }

    private static IServiceCollection AddAuthenticationInternal(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        JwtOptions jwtOptions = configuration
            .GetSection(JwtOptions.SectionName)
            .Get<JwtOptions>() ?? new JwtOptions();

        RefreshTokenOptions refreshTokenOptions = configuration
            .GetSection(RefreshTokenOptions.SectionName)
            .Get<RefreshTokenOptions>() ?? new RefreshTokenOptions();

        if (string.IsNullOrWhiteSpace(jwtOptions.Secret) &&
            string.Equals(configuration["ASPNETCORE_ENVIRONMENT"], "Testing", StringComparison.OrdinalIgnoreCase))
        {
            jwtOptions.Secret = "test-super-duper-secret-value-with-32-chars";
            jwtOptions.Issuer = "test-issuer";
            jwtOptions.Audience = "test-audience";
            jwtOptions.ExpirationInMinutes = 60;
        }

        ValidateJwtOptions(jwtOptions);
        ValidateRefreshTokenOptions(refreshTokenOptions);

        services.AddSingleton(jwtOptions);
        services.AddSingleton(refreshTokenOptions);

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(o =>
            {
                o.RequireHttpsMetadata = false;
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret)),
                    ValidateIssuer = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtOptions.Audience,
                    ValidateLifetime = true,
                    NameClaimType = ClaimTypes.NameIdentifier,
                    ClockSkew = TimeSpan.Zero
                };
            });

        services.AddHttpContextAccessor();
        services.AddScoped<IUserContext, UserContext>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<ITokenProvider, TokenProvider>();
        services.AddSingleton<IRefreshTokenProvider, RefreshTokenProvider>();
        services.AddSingleton<ITokenLifetimeProvider, TokenLifetimeProvider>();

        return services;
    }

    private static IServiceCollection AddAuthorizationInternal(
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

    private static void ValidateJwtOptions(JwtOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.Secret) || options.Secret.Length < 32)
        {
            throw new InvalidOperationException("Jwt:Secret must be at least 32 characters.");
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

    private static void ValidateRefreshTokenOptions(RefreshTokenOptions options)
    {
        if (options.ExpirationInDays is < 1 or > 180)
        {
            throw new InvalidOperationException("RefreshToken:ExpirationInDays must be between 1 and 180.");
        }
    }
}
