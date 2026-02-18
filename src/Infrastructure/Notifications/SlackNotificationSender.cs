using System.Net.Http.Json;
using Domain.Notifications;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Notifications;

internal sealed class SlackNotificationSender(
    IHttpClientFactory httpClientFactory,
    NotificationOptions options,
    ILogger<SlackNotificationSender> logger) : INotificationChannelSender
{
    public NotificationChannel Channel => NotificationChannel.Slack;

    public async Task<NotificationDispatchResult> SendAsync(
        NotificationMessage message,
        string recipient,
        CancellationToken cancellationToken)
    {
        NotificationSlackOptions slack = options.Slack;
        if (!slack.Enabled)
        {
            return new NotificationDispatchResult(false, null, "Slack sender is disabled.");
        }

        string text = BuildText(message);
        try
        {
            if (IsHttpUrl(recipient) || IsHttpUrl(slack.WebhookUrl))
            {
                string webhookUrl = IsHttpUrl(recipient) ? recipient : slack.WebhookUrl;
                HttpClient client = httpClientFactory.CreateClient(NotificationHttpClientNames.Slack);
                using HttpResponseMessage response = await client.PostAsJsonAsync(
                    webhookUrl,
                    new { text },
                    cancellationToken);

                string? body = await response.ReadBodyOrNullAsync(cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    return new NotificationDispatchResult(false, null, $"Slack webhook returned {(int)response.StatusCode}: {body}");
                }

                return new NotificationDispatchResult(true, $"slack-webhook-{Guid.NewGuid():N}", body);
            }

            if (string.IsNullOrWhiteSpace(slack.BotToken))
            {
                return new NotificationDispatchResult(false, null, "Slack BotToken is missing and webhook url is not provided.");
            }

            HttpClient apiClient = httpClientFactory.CreateClient(NotificationHttpClientNames.Slack);
            using HttpRequestMessage request = new(HttpMethod.Post, slack.PostMessageApiUrl)
            {
                Content = JsonContent.Create(new
                {
                    channel = recipient,
                    text
                })
            };
            request.ApplyApiKey(slack.BotToken, "Authorization", true);

            using HttpResponseMessage apiResponse = await apiClient.SendAsync(request, cancellationToken);
            string? apiBody = await apiResponse.ReadBodyOrNullAsync(cancellationToken);
            if (!apiResponse.IsSuccessStatusCode)
            {
                return new NotificationDispatchResult(false, null, $"Slack API returned {(int)apiResponse.StatusCode}: {apiBody}");
            }

            return new NotificationDispatchResult(true, $"slack-api-{Guid.NewGuid():N}", apiBody);
        }
        catch (Exception ex)
        {
            logger.LogWarning(
                ex,
                "Slack notification sending failed. NotificationId={NotificationId} RecipientHash={RecipientHash}",
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

    private static string BuildText(NotificationMessage message)
    {
        if (string.IsNullOrWhiteSpace(message.Subject))
        {
            return message.Body ?? string.Empty;
        }

        if (string.IsNullOrWhiteSpace(message.Body))
        {
            return message.Subject;
        }

        return $"{message.Subject}\n{message.Body}";
    }
}
