using Infrastructure.Auditing;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

internal static class AuditModule
{
    internal static IServiceCollection AddAuditModule(this IServiceCollection services)
    {
        services.AddScoped<IAuditTrailService, AuditTrailService>();
        return services;
    }
}
