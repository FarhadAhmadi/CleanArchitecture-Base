using Domain.Modules.Notifications;
using Domain.Notifications;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Notifications;

public sealed class NotificationDispatcher
{
    private readonly ApplicationDbContext _dbContext;
    private readonly NotificationSensitiveDataProtector _protector;
    private readonly Dictionary<NotificationChannel, INotificationChannelSender> _senders;
    private readonly NotificationOptions _options;
    private readonly ILogger<NotificationDispatcher> _logger;

    public NotificationDispatcher(
        ApplicationDbContext dbContext,
        NotificationSensitiveDataProtector protector,
        IEnumerable<INotificationChannelSender> senders,
        NotificationOptions options,
        ILogger<NotificationDispatcher> logger)
    {
        _dbContext = dbContext;
        _protector = protector;
        _options = options;
        _logger = logger;
        _senders = senders.ToDictionary(x => x.Channel);
    }

    public async Task<int> DispatchPendingAsync(CancellationToken cancellationToken)
    {
        DateTime utcNow = DateTime.UtcNow;

        List<NotificationMessage> candidates = await _dbContext.NotificationMessages
            .Where(x =>
                !x.IsArchived &&
                (x.Status == NotificationStatus.Pending || x.Status == NotificationStatus.Scheduled || x.Status == NotificationStatus.Failed) &&
                (x.NextRetryAtUtc == null || x.NextRetryAtUtc <= utcNow) &&
                (x.ScheduledAtUtc == null || x.ScheduledAtUtc <= utcNow))
            .OrderBy(x => x.Priority)
            .ThenBy(x => x.CreatedAtUtc)
            .Take(Math.Max(1, _options.DispatchBatchSize))
            .ToListAsync(cancellationToken);

        int processed = 0;
        foreach (NotificationMessage message in candidates)
        {
            processed++;
            await DispatchOneAsync(message, cancellationToken);
        }

        if (processed > 0)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return processed;
    }

    private async Task DispatchOneAsync(NotificationMessage message, CancellationToken cancellationToken)
    {
        if (!_senders.TryGetValue(message.Channel, out INotificationChannelSender? sender))
        {
            message.Status = NotificationStatus.Failed;
            message.LastError = $"Sender for channel {message.Channel} not found.";
            message.NextRetryAtUtc = null;
            return;
        }

        string recipient = _protector.Unprotect(message.RecipientEncrypted);
        NotificationDispatchResult result;

        try
        {
            result = await sender.SendAsync(message, recipient, cancellationToken);
        }
        catch (Exception ex)
        {
            result = new NotificationDispatchResult(false, null, ex.Message);
        }

        int attemptNumber = message.RetryCount + 1;
        _dbContext.NotificationDeliveryAttempts.Add(new NotificationDeliveryAttempt
        {
            Id = Guid.NewGuid(),
            NotificationId = message.Id,
            Channel = message.Channel,
            AttemptNumber = attemptNumber,
            IsSuccess = result.IsSuccess,
            ProviderMessageId = result.ProviderMessageId,
            Error = result.Error,
            CreatedAtUtc = DateTime.UtcNow
        });

        if (result.IsSuccess)
        {
            message.RetryCount = attemptNumber;
            message.Status = NotificationStatus.Sent;
            message.SentAtUtc = DateTime.UtcNow;
            message.DeliveredAtUtc = DateTime.UtcNow;
            message.Status = NotificationStatus.Delivered;
            message.LastError = null;
            message.NextRetryAtUtc = null;
            return;
        }

        message.RetryCount = attemptNumber;
        message.LastError = result.Error;

        if (message.RetryCount >= Math.Max(1, message.MaxRetryCount))
        {
            message.Status = NotificationStatus.Failed;
            message.NextRetryAtUtc = null;
            _logger.LogError(
                "Notification failed permanently. NotificationId={NotificationId} Channel={Channel} Error={Error}",
                message.Id,
                message.Channel,
                result.Error);
            return;
        }

        int delaySeconds = (int)(Math.Max(1, _options.BaseRetryDelaySeconds) * Math.Pow(2, message.RetryCount - 1));
        message.Status = NotificationStatus.Failed;
        message.NextRetryAtUtc = DateTime.UtcNow.AddSeconds(delaySeconds);
    }
}
