using Infrastructure.Notifications;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

internal static class NotificationsModule
{
    internal static IServiceCollection AddNotificationsModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        NotificationOptions options = configuration
            .GetSection(NotificationOptions.SectionName)
            .Get<NotificationOptions>() ?? new NotificationOptions();

        services.AddSingleton(options);
        services.AddSingleton<NotificationSensitiveDataProtector>();

        services.AddHttpClient(NotificationHttpClientNames.Sms, client =>
        {
            if (Uri.TryCreate(options.Sms.BaseUrl, UriKind.Absolute, out Uri? baseUri))
            {
                client.BaseAddress = baseUri;
            }
        });
        services.AddHttpClient(NotificationHttpClientNames.Slack);
        services.AddHttpClient(NotificationHttpClientNames.Teams);
        services.AddHttpClient(NotificationHttpClientNames.Push, client =>
        {
            if (Uri.TryCreate(options.Push.BaseUrl, UriKind.Absolute, out Uri? baseUri))
            {
                client.BaseAddress = baseUri;
            }
        });

        services.AddScoped<NotificationDispatcher>();
        services.AddHostedService<NotificationDispatchWorker>();

        services.AddSingleton<INotificationChannelSender, EmailNotificationSender>();
        services.AddSingleton<INotificationChannelSender, SmsNotificationSender>();
        services.AddSingleton<INotificationChannelSender, PushNotificationSender>();
        services.AddSingleton<INotificationChannelSender, InAppNotificationSender>();
        services.AddSingleton<INotificationChannelSender, SlackNotificationSender>();
        services.AddSingleton<INotificationChannelSender, TeamsNotificationSender>();

        return services;
    }
}
