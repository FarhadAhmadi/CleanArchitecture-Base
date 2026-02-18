using System.Net.Http.Headers;

namespace Infrastructure.Notifications;

internal static class NotificationSenderHttpExtensions
{
    internal static void ApplyApiKey(
        this HttpRequestMessage request,
        string apiKey,
        string headerName,
        bool useBearerToken)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return;
        }

        if (useBearerToken)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            return;
        }

        string name = string.IsNullOrWhiteSpace(headerName) ? "X-API-Key" : headerName.Trim();
        request.Headers.Remove(name);
        request.Headers.TryAddWithoutValidation(name, apiKey);
    }

    internal static async Task<string?> ReadBodyOrNullAsync(this HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.Content is null)
        {
            return null;
        }

        string body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(body))
        {
            return null;
        }

        return body.Length <= 600 ? body : body[..600];
    }
}
