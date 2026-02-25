using System.Net.Http.Json;
using Domain.Modules.Notifications;
using Domain.Notifications;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Notifications;

internal sealed class TeamsNotificationSender(
    IHttpClientFactory httpClientFactory,
    NotificationOptions options,
    ILogger<TeamsNotificationSender> logger) : INotificationChannelSender
{
    public NotificationChannel Channel => NotificationChannel.Teams;

    public async Task<NotificationDispatchResult> SendAsync(
        NotificationMessage message,
        string recipient,
        CancellationToken cancellationToken)
    {
        NotificationTeamsOptions teams = options.Teams;
        if (!teams.Enabled)
        {
            return new NotificationDispatchResult(false, null, "Teams sender is disabled.");
        }

        string webhookUrl = IsHttpUrl(recipient) ? recipient : teams.WebhookUrl;
        if (!IsHttpUrl(webhookUrl))
        {
            return new NotificationDispatchResult(false, null, "Teams webhook URL is invalid or missing.");
        }

        try
        {
            HttpClient client = httpClientFactory.CreateClient(NotificationHttpClientNames.Teams);
            using HttpResponseMessage response = await client.PostAsJsonAsync(
                webhookUrl,
                new
                {
                    title = string.IsNullOrWhiteSpace(message.Subject) ? "Notification" : message.Subject,
                    text = string.IsNullOrWhiteSpace(message.Body) ? message.Subject : message.Body
                },
                cancellationToken);

            string? body = await response.ReadBodyOrNullAsync(cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return new NotificationDispatchResult(false, null, $"Teams webhook returned {(int)response.StatusCode}: {body}");
            }

            return new NotificationDispatchResult(true, $"teams-{Guid.NewGuid():N}", body);
        }
        catch (Exception ex)
        {
            logger.LogWarning(
                ex,
                "Teams notification sending failed. NotificationId={NotificationId} RecipientHash={RecipientHash}",
                message.Id,
                message.RecipientHash ?? "n/a");
            return new NotificationDispatchResult(false, null, ex.Message);
        }
    }

    private static bool IsHttpUrl(string value)
    {
        return Uri.TryCreate(value, UriKind.Absolute, out Uri? uri) &&
               (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp);
    }
}
