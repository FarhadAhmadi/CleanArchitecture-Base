using System.Security.Claims;
using System.Threading.RateLimiting;
using System.Reflection;
using Asp.Versioning;
using Application.Abstractions.Files;
using Application.Abstractions.Messaging;
using Application.Abstractions.Notifications;
using Application.Abstractions.Observability;
using FluentValidation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Http.Resilience;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Web.Api.Endpoints.Files;
using Web.Api.Endpoints.Notifications;
using Web.Api.Infrastructure;

namespace Web.Api;

public static class DependencyInjection
{
    private const string DefaultCorsPolicy = "DefaultCors";
    private const string DefaultRateLimiterPolicy = "global";
    private const string PublicFileLinkRateLimiterPolicy = "files-public-link";
    private const string DefaultRequestTimeoutPolicy = "default-timeout";

    public static IServiceCollection AddPresentation(this IServiceCollection services, IConfiguration configuration)
    {
        ApiSecurityOptions apiSecurityOptions = configuration
            .GetSection(ApiSecurityOptions.SectionName)
            .Get<ApiSecurityOptions>() ?? new ApiSecurityOptions();

        TelemetryOptions telemetryOptions = configuration
            .GetSection(TelemetryOptions.SectionName)
            .Get<TelemetryOptions>() ?? new TelemetryOptions();

        Application.Abstractions.Observability.OperationalSloOptions operationalSloOptions = configuration
            .GetSection(Application.Abstractions.Observability.OperationalSloOptions.SectionName)
            .Get<Application.Abstractions.Observability.OperationalSloOptions>() ?? new Application.Abstractions.Observability.OperationalSloOptions();

        OperationalAlertingOptions operationalAlertingOptions = configuration
            .GetSection(OperationalAlertingOptions.SectionName)
            .Get<OperationalAlertingOptions>() ?? new OperationalAlertingOptions();

        ExternalOAuthOptions externalOAuthOptions = configuration
            .GetSection(ExternalOAuthOptions.SectionName)
            .Get<ExternalOAuthOptions>() ?? new ExternalOAuthOptions();

        services.AddSingleton(apiSecurityOptions);
        services.AddSingleton(telemetryOptions);
        services.AddSingleton(operationalSloOptions);
        services.AddSingleton(operationalAlertingOptions);
        services.AddSingleton(externalOAuthOptions);

        services.AddEndpointsApiExplorer();
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
            options.ApiVersionReader = new UrlSegmentApiVersionReader();
        });

        services.AddControllers();
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly, includeInternalTypes: true);
        services.AddScoped<IFileUseCaseService, FileUseCaseService>();
        services.AddScoped<INotificationUseCaseService, NotificationUseCaseService>();
        services.Scan(scan => scan.FromAssemblies(Assembly.GetExecutingAssembly())
            .AddClasses(classes => classes.AssignableTo(typeof(IQueryHandler<,>)), publicOnly: false)
                .AsImplementedInterfaces()
                .WithScopedLifetime()
            .AddClasses(classes => classes.AssignableTo(typeof(ICommandHandler<>)), publicOnly: false)
                .AsImplementedInterfaces()
                .WithScopedLifetime()
            .AddClasses(classes => classes.AssignableTo(typeof(ICommandHandler<,>)), publicOnly: false)
                .AsImplementedInterfaces()
                .WithScopedLifetime());

        AuthenticationBuilder authenticationBuilder = services.AddAuthentication();
        authenticationBuilder.AddCookie(ExternalAuthSchemes.ExternalCookie, options =>
        {
            options.Cookie.Name = "__external-auth";
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.ExpireTimeSpan = TimeSpan.FromMinutes(10);
            options.SlidingExpiration = false;
        });

        if (externalOAuthOptions.Google.Enabled &&
            !string.IsNullOrWhiteSpace(externalOAuthOptions.Google.ClientId) &&
            !string.IsNullOrWhiteSpace(externalOAuthOptions.Google.ClientSecret))
        {
            authenticationBuilder.AddGoogle(ExternalAuthSchemes.Google, options =>
            {
                options.SignInScheme = ExternalAuthSchemes.ExternalCookie;
                options.ClientId = externalOAuthOptions.Google.ClientId;
                options.ClientSecret = externalOAuthOptions.Google.ClientSecret;
            });
        }

        if (externalOAuthOptions.Meta.Enabled &&
            !string.IsNullOrWhiteSpace(externalOAuthOptions.Meta.AppId) &&
            !string.IsNullOrWhiteSpace(externalOAuthOptions.Meta.AppSecret))
        {
            authenticationBuilder.AddFacebook(ExternalAuthSchemes.Meta, options =>
            {
                options.SignInScheme = ExternalAuthSchemes.ExternalCookie;
                options.AppId = externalOAuthOptions.Meta.AppId;
                options.AppSecret = externalOAuthOptions.Meta.AppSecret;
            });
        }

        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        services.AddCors(options =>
        {
            options.AddPolicy(DefaultCorsPolicy, policy =>
            {
                bool isDevelopmentOrTesting =
                    string.Equals(configuration["ASPNETCORE_ENVIRONMENT"], "Development", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(configuration["ASPNETCORE_ENVIRONMENT"], "Testing", StringComparison.OrdinalIgnoreCase);

                if (apiSecurityOptions.AllowedOrigins.Length == 0)
                {
                    if (isDevelopmentOrTesting)
                    {
                        policy.WithOrigins("http://localhost:5000", "https://localhost:5001")
                            .AllowAnyHeader()
                            .AllowAnyMethod();
                    }
                    else
                    {
                        // Fail-closed for cross-origin requests when origins are not explicitly configured.
                        policy.SetIsOriginAllowed(_ => false)
                            .AllowAnyHeader()
                            .AllowAnyMethod();
                    }
                }
                else
                {
                    policy.WithOrigins(apiSecurityOptions.AllowedOrigins)
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                }
            });
        });

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
            {
                string userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
                string ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                string key = string.IsNullOrWhiteSpace(userId) ? $"ip:{ip}" : $"user:{userId}";
                int permitLimit = string.IsNullOrWhiteSpace(userId)
                    ? apiSecurityOptions.PerIpRateLimitPermitLimit
                    : apiSecurityOptions.PerUserRateLimitPermitLimit;

                return RateLimitPartition.GetFixedWindowLimiter(key, _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = Math.Max(1, permitLimit),
                    Window = TimeSpan.FromSeconds(apiSecurityOptions.RateLimitWindowSeconds),
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 0
                });
            });

            options.AddFixedWindowLimiter(DefaultRateLimiterPolicy, limiterOptions =>
            {
                limiterOptions.PermitLimit = apiSecurityOptions.RateLimitPermitLimit;
                limiterOptions.Window = TimeSpan.FromSeconds(apiSecurityOptions.RateLimitWindowSeconds);
                limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                limiterOptions.QueueLimit = 0;
            });

            options.AddPolicy<string>(PublicFileLinkRateLimiterPolicy, httpContext =>
            {
                string ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                return RateLimitPartition.GetFixedWindowLimiter(
                    $"public-file-link:ip:{ip}",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = Math.Max(1, apiSecurityOptions.PublicFileLinkRateLimitPermitLimit),
                        Window = TimeSpan.FromSeconds(Math.Max(1, apiSecurityOptions.PublicFileLinkRateLimitWindowSeconds)),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0
                    });
            });
        });

        services.AddRequestTimeouts(options =>
        {
            options.DefaultPolicy = new Microsoft.AspNetCore.Http.Timeouts.RequestTimeoutPolicy
            {
                Timeout = TimeSpan.FromSeconds(apiSecurityOptions.RequestTimeoutSeconds)
            };
            options.AddPolicy(DefaultRequestTimeoutPolicy, TimeSpan.FromSeconds(apiSecurityOptions.RequestTimeoutSeconds));
        });

        services.AddOpenTelemetry()
            .ConfigureResource(resource =>
            {
                resource.AddService(telemetryOptions.ServiceName);
            })
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation();

                if (!string.IsNullOrWhiteSpace(telemetryOptions.OtlpEndpoint))
                {
                    tracing.AddOtlpExporter(otlp =>
                    {
                        otlp.Endpoint = new Uri(telemetryOptions.OtlpEndpoint);
                    });
                }
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation();

                if (!string.IsNullOrWhiteSpace(telemetryOptions.OtlpEndpoint))
                {
                    metrics.AddOtlpExporter(otlp =>
                    {
                        otlp.Endpoint = new Uri(telemetryOptions.OtlpEndpoint);
                    });
                }
            });

        services.AddHttpClient("default")
            .AddStandardResilienceHandler(handler =>
            {
                handler.Retry.MaxRetryAttempts = 3;
                handler.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(30);
                handler.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(30);
            });

        services.AddHostedService<OperationalAlertWorker>();

        return services;
    }

    public static string GetCorsPolicyName() => DefaultCorsPolicy;

    public static string GetRateLimiterPolicyName() => DefaultRateLimiterPolicy;

    public static string GetPublicFileLinkRateLimiterPolicyName() => PublicFileLinkRateLimiterPolicy;
}

