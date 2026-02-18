using System.Net.Http.Json;
using Domain.Notifications;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Notifications;

internal sealed class PushNotificationSender(
    IHttpClientFactory httpClientFactory,
    NotificationOptions options,
    ILogger<PushNotificationSender> logger) : INotificationChannelSender
{
    public NotificationChannel Channel => NotificationChannel.Push;

    public async Task<NotificationDispatchResult> SendAsync(
        NotificationMessage message,
        string recipient,
        CancellationToken cancellationToken)
    {
        NotificationPushOptions push = options.Push;
        if (!push.Enabled)
        {
            return new NotificationDispatchResult(false, null, "Push sender is disabled.");
        }

        if (string.IsNullOrWhiteSpace(push.BaseUrl))
        {
            return new NotificationDispatchResult(false, null, "Push BaseUrl is not configured.");
        }

        if (string.IsNullOrWhiteSpace(recipient))
        {
            return new NotificationDispatchResult(false, null, "Push recipient token is required.");
        }

        try
        {
            HttpClient client = httpClientFactory.CreateClient(NotificationHttpClientNames.Push);
            using HttpRequestMessage request = new(HttpMethod.Post, string.IsNullOrWhiteSpace(push.EndpointPath) ? "send" : push.EndpointPath.TrimStart('/'))
            {
                Content = JsonContent.Create(new
                {
                    to = recipient,
                    title = string.IsNullOrWhiteSpace(message.Subject) ? "Notification" : message.Subject,
                    body = message.Body ?? string.Empty
                })
            };
            request.ApplyApiKey(push.ApiKey, push.ApiKeyHeaderName, push.UseBearerToken);

            using HttpResponseMessage response = await client.SendAsync(request, cancellationToken);
            string? body = await response.ReadBodyOrNullAsync(cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return new NotificationDispatchResult(false, null, $"Push provider returned {(int)response.StatusCode}: {body}");
            }

            return new NotificationDispatchResult(true, $"push-{Guid.NewGuid():N}", body);
        }
        catch (Exception ex)
        {
            logger.LogWarning(
                ex,
                "Push notification sending failed. NotificationId={NotificationId} RecipientHash={RecipientHash}",
                message.Id,
                message.RecipientHash ?? "n/a");
            return new NotificationDispatchResult(false, null, ex.Message);
        }
    }
}
