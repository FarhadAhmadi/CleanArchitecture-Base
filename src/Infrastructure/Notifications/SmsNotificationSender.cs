using System.Net.Http.Json;
using Domain.Notifications;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Notifications;

internal sealed class SmsNotificationSender(
    IHttpClientFactory httpClientFactory,
    NotificationOptions options,
    ILogger<SmsNotificationSender> logger) : INotificationChannelSender
{
    public NotificationChannel Channel => NotificationChannel.Sms;

    public async Task<NotificationDispatchResult> SendAsync(
        NotificationMessage message,
        string recipient,
        CancellationToken cancellationToken)
    {
        NotificationSmsOptions sms = options.Sms;
        if (!sms.Enabled)
        {
            return new NotificationDispatchResult(false, null, "SMS sender is disabled.");
        }

        if (string.IsNullOrWhiteSpace(sms.BaseUrl))
        {
            return new NotificationDispatchResult(false, null, "SMS BaseUrl is not configured.");
        }

        if (!IsValidRecipient(recipient))
        {
            return new NotificationDispatchResult(false, null, "Recipient phone number is invalid.");
        }

        try
        {
            HttpClient client = httpClientFactory.CreateClient(NotificationHttpClientNames.Sms);
            using HttpRequestMessage request = new(HttpMethod.Post, string.IsNullOrWhiteSpace(sms.EndpointPath) ? "send" : sms.EndpointPath.TrimStart('/'))
            {
                Content = JsonContent.Create(new
                {
                    to = recipient,
                    senderId = sms.SenderId,
                    message = BuildMessage(message),
                    subject = message.Subject
                })
            };
            request.ApplyApiKey(sms.ApiKey, sms.ApiKeyHeaderName, sms.UseBearerToken);

            using HttpResponseMessage response = await client.SendAsync(request, cancellationToken);
            string? body = await response.ReadBodyOrNullAsync(cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return new NotificationDispatchResult(false, null, $"SMS provider returned {(int)response.StatusCode}: {body}");
            }

            return new NotificationDispatchResult(true, $"sms-{Guid.NewGuid():N}", body);
        }
        catch (Exception ex)
        {
            logger.LogWarning(
                ex,
                "Sms notification sending failed. NotificationId={NotificationId} RecipientHash={RecipientHash}",
                message.Id,
                message.RecipientHash ?? "n/a");
            return new NotificationDispatchResult(false, null, ex.Message);
        }
    }

    private static bool IsValidRecipient(string recipient)
    {
        if (string.IsNullOrWhiteSpace(recipient))
        {
            return false;
        }

        int digitCount = recipient.Count(char.IsDigit);
        return digitCount is >= 8 and <= 20;
    }

    private static string BuildMessage(NotificationMessage message)
    {
        if (!string.IsNullOrWhiteSpace(message.Body))
        {
            return message.Body;
        }

        return string.IsNullOrWhiteSpace(message.Subject) ? "(no-content)" : message.Subject;
    }
}
