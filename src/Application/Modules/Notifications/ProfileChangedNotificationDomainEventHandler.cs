using Application.Abstractions.Notifications;
using Domain.Modules.Notifications;
using Domain.Notifications;
using Domain.Profiles;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace Application.Notifications;

internal sealed class ProfileChangedNotificationDomainEventHandler(
    INotificationMessageWriter notificationMessageWriter,
    ILogger<ProfileChangedNotificationDomainEventHandler> logger,
    IDateTimeProvider dateTimeProvider)
    : IDomainEventHandler<UserProfileChangedDomainEvent>
{
    public async Task Handle(UserProfileChangedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        DateTime now = dateTimeProvider.UtcNow;
        string traceId = domainEvent.ProfileId.ToString("N");
        string recipient = domainEvent.UserId.ToString("N");
        Guid notificationId = CreateDeterministicGuid(
            $"profiles.notification|{traceId}|{domainEvent.ChangeType}|{domainEvent.CompletenessScore}");

        bool queued = await notificationMessageWriter.TryQueueAsync(
            new NotificationMessageDraft(
                Id: notificationId,
                CreatedByUserId: domainEvent.UserId,
                Channel: NotificationChannel.InApp,
                Priority: NotificationPriority.Low,
                Status: NotificationStatus.Pending,
                RecipientRaw: recipient,
                Subject: "Profile updated",
                Body: $"Your profile was updated ({domainEvent.ChangeType}). Completion score: {domainEvent.CompletenessScore}%.",
                Language: "fa-IR",
                CreatedAtUtc: now,
                MaxRetryCount: 1),
            cancellationToken);

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug(
                "ProfileChanged notification fanout handled. ProfileId={ProfileId} NotificationId={NotificationId} Queued={Queued}",
                domainEvent.ProfileId,
                notificationId,
                queued);
        }
    }

    private static Guid CreateDeterministicGuid(string value)
    {
        byte[] hash = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(value));
        Span<byte> bytes = stackalloc byte[16];
        hash.AsSpan(0, 16).CopyTo(bytes);
        return new Guid(bytes);
    }
}
