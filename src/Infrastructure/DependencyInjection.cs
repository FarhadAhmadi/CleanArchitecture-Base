using System.Security.Claims;
using System.Text;
using Application.Abstractions.Authentication;
using Application.Abstractions.Authorization;
using Application.Abstractions.Data;
using Application.Abstractions.Security;
using Infrastructure.Authentication;
using Infrastructure.Auditing;
using Infrastructure.Caching;
using Infrastructure.Authorization;
using Infrastructure.Database;
using Infrastructure.DomainEvents;
using Infrastructure.Integration;
using Infrastructure.Logging;
using Infrastructure.Messaging;
using Infrastructure.Monitoring;
using Infrastructure.Security;
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
            .AddCaching(configuration)
            .AddIntegration(configuration)
            .AddHealthChecks(configuration)
            .AddLoggingPlatform(configuration)
            .AddAuthenticationInternal(configuration)
            .AddAuthorizationInternal(configuration);

    private static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.AddTransient<IDomainEventsDispatcher, DomainEventsDispatcher>();
        services.AddScoped<IAuditTrailService, AuditTrailService>();
        services.AddSingleton<ISecurityEventLogger, SecurityEventLogger>();
        services.AddScoped<OperationalMetricsService>();
        return services;
    }

    private static IServiceCollection AddDatabase(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        string? connectionString = configuration.GetConnectionString("Database");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.MigrationsHistoryTable(
                    HistoryRepository.DefaultTableName,
                    Schemas.Default);
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(15),
                    errorNumbersToAdd: null);
            }));

        services.AddScoped<IApplicationDbContext>(sp =>
            sp.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<IApplicationReadDbContext, ApplicationReadDbContext>();

        return services;
    }

    private static IServiceCollection AddCaching(this IServiceCollection services, IConfiguration configuration)
    {
        RedisCacheOptions redisOptions = configuration
            .GetSection(RedisCacheOptions.SectionName)
            .Get<RedisCacheOptions>() ?? new RedisCacheOptions();

        PermissionCacheOptions permissionCacheOptions = configuration
            .GetSection(PermissionCacheOptions.SectionName)
            .Get<PermissionCacheOptions>() ?? new PermissionCacheOptions();

        services.AddSingleton(redisOptions);
        services.AddSingleton(permissionCacheOptions);

        if (redisOptions.Enabled && !string.IsNullOrWhiteSpace(redisOptions.ConnectionString))
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisOptions.ConnectionString;
                options.InstanceName = redisOptions.InstanceName;
            });
        }
        else
        {
            services.AddDistributedMemoryCache();
        }

        services.AddSingleton<IPermissionCacheVersionService, PermissionCacheVersionService>();

        return services;
    }

    private static IServiceCollection AddHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddHealthChecks()
            .AddSqlServer(configuration.GetConnectionString("Database")!)
            .AddCheck<OutboxHealthCheck>("outbox");

        return services;
    }

    private static IServiceCollection AddIntegration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        OutboxOptions outboxOptions = configuration
            .GetSection(OutboxOptions.SectionName)
            .Get<OutboxOptions>() ?? new OutboxOptions();

        RabbitMqOptions rabbitMqOptions = configuration
            .GetSection(RabbitMqOptions.SectionName)
            .Get<RabbitMqOptions>() ?? new RabbitMqOptions();

        services.AddSingleton(outboxOptions);
        services.AddSingleton(rabbitMqOptions);
        services.AddSingleton<IntegrationEventSerializer>();
        services.AddSingleton<IIntegrationEventSerializer>(sp => sp.GetRequiredService<IntegrationEventSerializer>());
        services.AddSingleton<IIntegrationEventPublisher, RabbitMqIntegrationEventPublisher>();
        services.AddScoped<IInboxStore, InboxStore>();
        services.AddHostedService<OutboxProcessorWorker>();
        services.AddHostedService<RabbitMqInboxWorker>();

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

        AuthSecurityOptions authSecurityOptions = configuration
            .GetSection(AuthSecurityOptions.SectionName)
            .Get<AuthSecurityOptions>() ?? new AuthSecurityOptions();

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
        services.AddSingleton(authSecurityOptions);

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(o =>
            {
                List<SecurityKey> signingKeys =
                [
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret))
                ];

                foreach (string previousSecret in jwtOptions.PreviousSecrets.Where(x => !string.IsNullOrWhiteSpace(x)))
                {
                    signingKeys.Add(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(previousSecret)));
                }

                o.RequireHttpsMetadata = false;
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

    private static void ValidateRefreshTokenOptions(RefreshTokenOptions options)
    {
        if (options.ExpirationInDays is < 1 or > 180)
        {
            throw new InvalidOperationException("RefreshToken:ExpirationInDays must be between 1 and 180.");
        }
    }
}
