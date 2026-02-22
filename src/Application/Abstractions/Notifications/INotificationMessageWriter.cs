using Domain.Modules.Notifications;
using Domain.Notifications;

namespace Application.Abstractions.Notifications;

public sealed record NotificationMessageDraft(
    Guid Id,
    Guid CreatedByUserId,
    NotificationChannel Channel,
    NotificationPriority Priority,
    NotificationStatus Status,
    string RecipientRaw,
    string Subject,
    string Body,
    string Language,
    DateTime CreatedAtUtc,
    int MaxRetryCount,
    Guid? TemplateId = null,
    DateTime? ScheduledAtUtc = null);

public interface INotificationMessageWriter
{
    Task<bool> TryQueueAsync(NotificationMessageDraft draft, CancellationToken cancellationToken);
    Task<int> QueueManyAsync(IEnumerable<NotificationMessageDraft> drafts, CancellationToken cancellationToken);
}
