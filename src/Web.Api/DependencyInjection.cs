using System.Threading.RateLimiting;
using Asp.Versioning;
using Microsoft.AspNetCore.RateLimiting;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Web.Api.Infrastructure;

namespace Web.Api;

public static class DependencyInjection
{
    private const string DefaultCorsPolicy = "DefaultCors";
    private const string DefaultRateLimiterPolicy = "global";
    private const string DefaultRequestTimeoutPolicy = "default-timeout";

    public static IServiceCollection AddPresentation(this IServiceCollection services, IConfiguration configuration)
    {
        ApiSecurityOptions apiSecurityOptions = configuration
            .GetSection(ApiSecurityOptions.SectionName)
            .Get<ApiSecurityOptions>() ?? new ApiSecurityOptions();

        TelemetryOptions telemetryOptions = configuration
            .GetSection(TelemetryOptions.SectionName)
            .Get<TelemetryOptions>() ?? new TelemetryOptions();

        services.AddSingleton(apiSecurityOptions);
        services.AddSingleton(telemetryOptions);

        services.AddEndpointsApiExplorer();
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
            options.ApiVersionReader = new UrlSegmentApiVersionReader();
        });

        services.AddControllers();

        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        services.AddCors(options =>
        {
            options.AddPolicy(DefaultCorsPolicy, policy =>
            {
                if (apiSecurityOptions.AllowedOrigins.Length == 0)
                {
                    policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
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
            options.AddFixedWindowLimiter(DefaultRateLimiterPolicy, limiterOptions =>
            {
                limiterOptions.PermitLimit = apiSecurityOptions.RateLimitPermitLimit;
                limiterOptions.Window = TimeSpan.FromSeconds(apiSecurityOptions.RateLimitWindowSeconds);
                limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                limiterOptions.QueueLimit = 0;
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

        return services;
    }

    public static string GetCorsPolicyName() => DefaultCorsPolicy;

    public static string GetRateLimiterPolicyName() => DefaultRateLimiterPolicy;
}
