using System.Reflection;
using System.Security.Claims;
using System.Globalization;
using Application;
using Azure.Identity;
using Infrastructure;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Elasticsearch;
using Web.Api;
using Web.Api.Endpoints.Logging;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

SecretManagementOptions secretManagementOptions = builder.Configuration
    .GetSection(SecretManagementOptions.SectionName)
    .Get<SecretManagementOptions>() ?? new SecretManagementOptions();

if (secretManagementOptions.UseAzureKeyVault &&
    !string.IsNullOrWhiteSpace(secretManagementOptions.KeyVaultUri))
{
    builder.Configuration.AddAzureKeyVault(
        new Uri(secretManagementOptions.KeyVaultUri),
        new DefaultAzureCredential());
}

ApiSecurityOptions apiSecurityOptions = builder.Configuration
    .GetSection(ApiSecurityOptions.SectionName)
    .Get<ApiSecurityOptions>() ?? new ApiSecurityOptions();

DatabaseMigrationOptions migrationOptions = builder.Configuration
    .GetSection(DatabaseMigrationOptions.SectionName)
    .Get<DatabaseMigrationOptions>() ?? new DatabaseMigrationOptions();

builder.WebHost.ConfigureKestrel(options =>
{
    options.AddServerHeader = !apiSecurityOptions.HideServerHeader;
    options.Limits.MaxRequestBodySize = apiSecurityOptions.MaxRequestBodySizeMb * 1024L * 1024L;
    options.Limits.MaxRequestHeadersTotalSize = Math.Max(1, apiSecurityOptions.MaxRequestHeadersTotalSizeKb) * 1024;
    options.Limits.RequestHeadersTimeout = TimeSpan.FromSeconds(Math.Max(1, apiSecurityOptions.RequestHeadersTimeoutSeconds));
});

builder.Host.UseSerilog((context, loggerConfig) =>
{
    loggerConfig.ReadFrom.Configuration(context.Configuration);

    LogSinkOptions sinkOptions = context.Configuration
        .GetSection(LogSinkOptions.SectionName)
        .Get<LogSinkOptions>() ?? new LogSinkOptions();

    if (sinkOptions.EnableConsole)
    {
        loggerConfig.WriteTo.Console(formatProvider: CultureInfo.InvariantCulture);
    }

    if (string.Equals(sinkOptions.Provider, "elasticsearch", StringComparison.OrdinalIgnoreCase))
    {
        if (!string.IsNullOrWhiteSpace(sinkOptions.ElasticsearchNodeUrl))
        {
            loggerConfig.WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(sinkOptions.ElasticsearchNodeUrl))
            {
                IndexFormat = sinkOptions.ElasticsearchIndexFormat,
                AutoRegisterTemplate = true
            });
        }
    }
    else
    {
        if (!string.IsNullOrWhiteSpace(sinkOptions.SeqServerUrl))
        {
            loggerConfig.WriteTo.Seq(sinkOptions.SeqServerUrl, formatProvider: CultureInfo.InvariantCulture);
        }
    }
});

builder.Services.AddSwaggerGenWithAuth();

builder.Services
    .AddApplication()
    .AddPresentation(builder.Configuration)
    .AddInfrastructure(builder.Configuration);

builder.Services.AddEndpoints(Assembly.GetExecutingAssembly());

WebApplication app = builder.Build();

RouteGroupBuilder v1 = app
    .MapGroup("api/v1")
    .AddEndpointFilterFactory(EndpointExecutionLoggingFilter.Create)
    .AddEndpointFilterFactory(RequestSanitizationEndpointFilter.Create);

app.MapEndpoints(v1);
app.MapLoggingEndpoints();

if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Testing"))
{
    app.UseSwaggerWithUi();
}
await app.ApplyMigrationsIfEnabledAsync(migrationOptions);

app.MapHealthChecks("health", new HealthCheckOptions
{
    ResponseWriter = static async (httpContext, report) =>
    {
        httpContext.Response.ContentType = "application/json";
        await httpContext.Response.WriteAsJsonAsync(new
        {
            status = report.Status.ToString()
        });
    }
});

app.UseHttpsRedirection();
app.UseRequestContextLogging();
app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
    options.GetLevel = (httpContext, _, exception) =>
    {
        if (exception is not null || httpContext.Response.StatusCode >= StatusCodes.Status500InternalServerError)
        {
            return LogEventLevel.Error;
        }

        if (httpContext.Response.StatusCode >= StatusCodes.Status400BadRequest)
        {
            return LogEventLevel.Warning;
        }

        return LogEventLevel.Information;
    };
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("UserId", httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous");
        diagnosticContext.Set("ClientIp", httpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty);
        diagnosticContext.Set("RequestPath", httpContext.Request.Path.Value ?? string.Empty);
        diagnosticContext.Set("RequestMethod", httpContext.Request.Method);
        diagnosticContext.Set(
            "RequestQueryString",
            httpContext.Request.QueryString.HasValue ? httpContext.Request.QueryString.Value : string.Empty);
        diagnosticContext.Set("EndpointName", httpContext.GetEndpoint()?.DisplayName ?? "unknown");
        diagnosticContext.Set("CorrelationId", httpContext.TraceIdentifier);
        diagnosticContext.Set("StatusCode", httpContext.Response.StatusCode);
    };
});
if (apiSecurityOptions.EnableSecurityHeaders)
{
    app.UseSecurityHeaders();
}

if (!app.Environment.IsDevelopment() && apiSecurityOptions.UseHsts)
{
    app.UseHsts();
}

app.UseExceptionHandler();
app.UseStatusCodePages(StatusCodePageExtensions.WriteProblemDetails);
app.UseRequestHardening();
app.UseRequestTimeouts();
app.UseCors(Web.Api.DependencyInjection.GetCorsPolicyName());
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

await app.RunAsync();

namespace Web.Api
{
    public partial class Program;
}

