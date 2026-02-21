using Domain.Modules.Notifications;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace Application.Notifications;

internal sealed class NotificationCreatedDomainEventHandler(
    ILogger<NotificationCreatedDomainEventHandler> logger)
    : IDomainEventHandler<NotificationCreatedDomainEvent>
{
    public Task Handle(NotificationCreatedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Notification created domain event. NotificationId={NotificationId} Channel={Channel} Priority={Priority} Status={Status}",
                domainEvent.NotificationId,
                domainEvent.Channel,
                domainEvent.Priority,
                domainEvent.Status);
        }

        return Task.CompletedTask;
    }
}

internal sealed class NotificationScheduledDomainEventHandler(
    ILogger<NotificationScheduledDomainEventHandler> logger)
    : IDomainEventHandler<NotificationScheduledDomainEvent>
{
    public Task Handle(NotificationScheduledDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Notification scheduled domain event. NotificationId={NotificationId} RunAtUtc={RunAtUtc}",
                domainEvent.NotificationId,
                domainEvent.RunAtUtc);
        }

        return Task.CompletedTask;
    }
}

internal sealed class NotificationArchivedDomainEventHandler(
    ILogger<NotificationArchivedDomainEventHandler> logger)
    : IDomainEventHandler<NotificationArchivedDomainEvent>
{
    public Task Handle(NotificationArchivedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Notification archived domain event. NotificationId={NotificationId} ArchivedAtUtc={ArchivedAtUtc}",
                domainEvent.NotificationId,
                domainEvent.ArchivedAtUtc);
        }

        return Task.CompletedTask;
    }
}
