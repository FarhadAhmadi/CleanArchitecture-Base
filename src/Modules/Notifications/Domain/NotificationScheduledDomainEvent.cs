using SharedKernel;

namespace Domain.Modules.Notifications;

public sealed record NotificationScheduledDomainEvent(
    Guid NotificationId,
    DateTime RunAtUtc) : IDomainEvent;
