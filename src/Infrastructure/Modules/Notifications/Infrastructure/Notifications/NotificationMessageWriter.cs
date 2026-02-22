using Application.Abstractions.Data;
using Application.Abstractions.Notifications;
using Domain.Modules.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Notifications;

internal sealed class NotificationMessageWriter(
    INotificationsWriteDbContext dbContext,
    NotificationSensitiveDataProtector protector,
    ILogger<NotificationMessageWriter> logger) : INotificationMessageWriter
{
    public async Task<bool> TryQueueAsync(NotificationMessageDraft draft, CancellationToken cancellationToken)
    {
        int inserted = await QueueCoreAsync([draft], cancellationToken);
        return inserted > 0;
    }

    public Task<int> QueueManyAsync(IEnumerable<NotificationMessageDraft> drafts, CancellationToken cancellationToken)
    {
        return QueueCoreAsync(drafts, cancellationToken);
    }

    private async Task<int> QueueCoreAsync(IEnumerable<NotificationMessageDraft> drafts, CancellationToken cancellationToken)
    {
        var items = drafts.ToList();
        if (items.Count == 0)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug("Notification queue request ignored because no draft was provided.");
            }
            return 0;
        }

        HashSet<Guid> ids = [.. items.Select(x => x.Id)];
        HashSet<Guid> existingIds = [.. await dbContext.NotificationMessages
            .AsNoTracking()
            .Where(x => ids.Contains(x.Id))
            .Select(x => x.Id)
            .ToListAsync(cancellationToken)];

        int inserted = 0;

        foreach (NotificationMessageDraft draft in items)
        {
            if (existingIds.Contains(draft.Id))
            {
                if (logger.IsEnabled(LogLevel.Debug))
                {
                    logger.LogDebug(
                        "Notification draft skipped because it is already queued. NotificationId={NotificationId}",
                        draft.Id);
                }
                continue;
            }

            string recipient = draft.RecipientRaw.Trim();
            if (string.IsNullOrWhiteSpace(recipient))
            {
                if (logger.IsEnabled(LogLevel.Warning))
                {
                    logger.LogWarning(
                        "Notification draft skipped because recipient is empty. NotificationId={NotificationId}",
                        draft.Id);
                }
                continue;
            }

            dbContext.NotificationMessages.Add(new NotificationMessage
            {
                Id = draft.Id,
                CreatedByUserId = draft.CreatedByUserId,
                Channel = draft.Channel,
                Priority = draft.Priority,
                Status = draft.Status,
                RecipientEncrypted = protector.Protect(recipient),
                RecipientHash = NotificationSensitiveDataProtector.ComputeDeterministicHash(recipient),
                Subject = draft.Subject,
                Body = draft.Body,
                Language = string.IsNullOrWhiteSpace(draft.Language) ? "fa-IR" : draft.Language.Trim(),
                TemplateId = draft.TemplateId,
                CreatedAtUtc = draft.CreatedAtUtc,
                ScheduledAtUtc = draft.ScheduledAtUtc,
                MaxRetryCount = Math.Max(1, draft.MaxRetryCount)
            });

            inserted++;
        }

        if (inserted > 0)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(
                    "Notification drafts queued. Inserted={Inserted} Requested={Requested}",
                    inserted,
                    items.Count);
            }
            return inserted;
        }

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug(
                "Notification queue request completed with no inserts. Requested={Requested}",
                items.Count);
        }
        return inserted;
    }
}
