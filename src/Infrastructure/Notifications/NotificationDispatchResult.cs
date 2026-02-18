namespace Infrastructure.Notifications;

public sealed record NotificationDispatchResult(bool IsSuccess, string? ProviderMessageId, string? Error);
