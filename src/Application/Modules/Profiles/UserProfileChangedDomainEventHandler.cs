using System.Security.Cryptography;
using System.Text;
using Application.Abstractions.Data;
using Domain.Logging;
using Domain.Notifications;
using Domain.Profiles;
using SharedKernel;

namespace Application.Profiles;

internal sealed class UserProfileChangedDomainEventHandler(
    IApplicationDbContext context,
    IDateTimeProvider dateTimeProvider)
    : IDomainEventHandler<UserProfileChangedDomainEvent>
{
    public async Task Handle(UserProfileChangedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        DateTime now = dateTimeProvider.UtcNow;

        context.LogEvents.Add(new LogEvent
        {
            Id = Guid.NewGuid(),
            TimestampUtc = now,
            Level = LogLevelType.Info,
            Message = $"Profile changed ({domainEvent.ChangeType})",
            SourceService = "Web.Api",
            SourceModule = "Profiles",
            TraceId = domainEvent.ProfileId.ToString("N"),
            ActorType = "User",
            ActorId = domainEvent.UserId.ToString("N"),
            Outcome = "Success",
            TagsCsv = "profile,domain-event",
            Checksum = ComputeChecksum(domainEvent, now)
        });

        string recipientRaw = domainEvent.UserId.ToString("N");
        context.NotificationMessages.Add(new NotificationMessage
        {
            Id = Guid.NewGuid(),
            CreatedByUserId = domainEvent.UserId,
            Channel = NotificationChannel.InApp,
            Priority = NotificationPriority.Low,
            Status = NotificationStatus.Pending,
            RecipientEncrypted = Convert.ToBase64String(Encoding.UTF8.GetBytes(recipientRaw)),
            RecipientHash = ComputeHash(recipientRaw),
            Subject = "Profile updated",
            Body = $"Your profile was updated ({domainEvent.ChangeType}). Completion score: {domainEvent.CompletenessScore}%.",
            Language = "fa-IR",
            CreatedAtUtc = now,
            MaxRetryCount = 1
        });

        await context.SaveChangesAsync(cancellationToken);
    }

    private static string ComputeChecksum(UserProfileChangedDomainEvent domainEvent, DateTime now)
    {
        string payload = $"{domainEvent.ProfileId:N}|{domainEvent.UserId:N}|{domainEvent.ChangeType}|{domainEvent.CompletenessScore}|{now:O}";
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexString(hash);
    }

    private static string ComputeHash(string value)
    {
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(hash);
    }
}
