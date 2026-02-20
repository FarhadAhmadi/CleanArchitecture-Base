using Infrastructure.Notifications;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Mail;

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

        ValidateOptions(options);

        services.AddSingleton(options);
        services.AddSingleton<NotificationSensitiveDataProtector>();
        services.AddScoped<NotificationTemplateRenderer>();
        services.AddScoped<NotificationTemplateSeeder>();

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

    private static void ValidateOptions(NotificationOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.SensitiveDataEncryptionKey))
        {
            throw new InvalidOperationException(
                "Notifications:SensitiveDataEncryptionKey is required for notification data protection.");
        }

        NotificationEmailOptions email = options.Email;
        if (!email.Enabled)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(email.Host))
        {
            throw new InvalidOperationException("Notifications:Email:Host is required when email sender is enabled.");
        }

        if (email.Port is <= 0 or > 65535)
        {
            throw new InvalidOperationException("Notifications:Email:Port must be between 1 and 65535.");
        }

        if (string.IsNullOrWhiteSpace(email.FromAddress))
        {
            throw new InvalidOperationException("Notifications:Email:FromAddress is required when email sender is enabled.");
        }

        try
        {
            _ = new MailAddress(email.FromAddress);
        }
        catch (FormatException)
        {
            throw new InvalidOperationException("Notifications:Email:FromAddress is not a valid email address.");
        }

        if (!string.IsNullOrWhiteSpace(email.UserName) && string.IsNullOrWhiteSpace(email.Password))
        {
            throw new InvalidOperationException(
                "Notifications:Email:Password is required when Notifications:Email:UserName is provided.");
        }
    }
}
