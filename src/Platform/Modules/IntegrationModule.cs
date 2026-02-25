using Infrastructure.Integration;
using Infrastructure.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

internal static class IntegrationModule
{
    internal static IServiceCollection AddIntegrationModule(
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
        services.AddSingleton<IIntegrationEventSerializer>(sp => sp.GetRequiredService<IntegrationEventSerializer>());
        services.AddSingleton<IIntegrationEventPublisher, RabbitMqIntegrationEventPublisher>();
        services.AddScoped<IInboxStore, InboxStore>();
        services.AddHostedService<OutboxProcessorWorker>();
        services.AddHostedService<RabbitMqInboxWorker>();

        return services;
    }
}
