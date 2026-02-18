using Infrastructure.DomainEvents;
using Infrastructure.Time;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel;

namespace Infrastructure;

internal static class CoreModule
{
    internal static IServiceCollection AddCoreModule(this IServiceCollection services)
    {
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.AddTransient<IDomainEventsDispatcher, DomainEventsDispatcher>();
        return services;
    }
}
