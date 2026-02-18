namespace Infrastructure.Notifications;

public sealed class NotificationOptions
{
    public const string SectionName = "Notifications";

    public bool Enabled { get; init; } = true;
    public int MaxRetries { get; init; } = 5;
    public int BaseRetryDelaySeconds { get; init; } = 10;
    public int DispatchBatchSize { get; init; } = 100;
    public int DispatchPollingSeconds { get; init; } = 5;
    public int PerUserPerMinuteLimit { get; init; } = 120;
    public string SensitiveDataEncryptionKey { get; init; } = string.Empty;

    public NotificationEmailOptions Email { get; init; } = new();
    public NotificationSmsOptions Sms { get; init; } = new();
    public NotificationSlackOptions Slack { get; init; } = new();
    public NotificationTeamsOptions Teams { get; init; } = new();
    public NotificationPushOptions Push { get; init; } = new();
    public NotificationInAppOptions InApp { get; init; } = new();
}

public sealed class NotificationEmailOptions
{
    public bool Enabled { get; init; }
    public string Host { get; init; } = string.Empty;
    public int Port { get; init; } = 587;
    public bool UseSsl { get; init; } = true;
    public string UserName { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string FromAddress { get; init; } = string.Empty;
    public string FromName { get; init; } = "NextGen Notifications";
    public bool IsBodyHtml { get; init; } = true;
}

public sealed class NotificationSmsOptions
{
    public bool Enabled { get; init; }
    public string BaseUrl { get; init; } = string.Empty;
    public string EndpointPath { get; init; } = "send";
    public string ApiKey { get; init; } = string.Empty;
    public string ApiKeyHeaderName { get; init; } = "X-API-Key";
    public bool UseBearerToken { get; init; } = true;
    public string SenderId { get; init; } = string.Empty;
}

public sealed class NotificationSlackOptions
{
    public bool Enabled { get; init; }
    public string WebhookUrl { get; init; } = string.Empty;
    public string BotToken { get; init; } = string.Empty;
    public string PostMessageApiUrl { get; init; } = "https://slack.com/api/chat.postMessage";
}

public sealed class NotificationTeamsOptions
{
    public bool Enabled { get; init; }
    public string WebhookUrl { get; init; } = string.Empty;
}

public sealed class NotificationPushOptions
{
    public bool Enabled { get; init; }
    public string BaseUrl { get; init; } = string.Empty;
    public string EndpointPath { get; init; } = "send";
    public string ApiKey { get; init; } = string.Empty;
    public string ApiKeyHeaderName { get; init; } = "X-API-Key";
    public bool UseBearerToken { get; init; } = true;
}

public sealed class NotificationInAppOptions
{
    public bool Enabled { get; init; } = true;
}
