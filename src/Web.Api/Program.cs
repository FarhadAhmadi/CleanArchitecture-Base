using System.Reflection;
using System.Security.Claims;
using Azure.Identity;
using Application;
using HealthChecks.UI.Client;
using Infrastructure;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;
using Web.Api.Endpoints.Logging;
using Web.Api;
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
    options.Limits.MaxRequestBodySize = apiSecurityOptions.MaxRequestBodySizeMb * 1024L * 1024L;
});

builder.Host.UseSerilog((context, loggerConfig) => loggerConfig.ReadFrom.Configuration(context.Configuration));

builder.Services.AddSwaggerGenWithAuth();

builder.Services
    .AddApplication()
    .AddPresentation(builder.Configuration)
    .AddInfrastructure(builder.Configuration);

builder.Services.AddEndpoints(Assembly.GetExecutingAssembly());

WebApplication app = builder.Build();

RouteGroupBuilder v1 = app
    .MapGroup("api/v1");

app.MapEndpoints(v1);
app.MapLoggingEndpoints();

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerWithUi();
}
app.ApplyMigrationsIfEnabled(migrationOptions);

app.MapHealthChecks("health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.UseRequestContextLogging();
app.UseSerilogRequestLogging(options =>
{
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("UserId", httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous");
        diagnosticContext.Set("ClientIp", httpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty);
        diagnosticContext.Set("RequestPath", httpContext.Request.Path.Value ?? string.Empty);
        diagnosticContext.Set("RequestMethod", httpContext.Request.Method);
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
