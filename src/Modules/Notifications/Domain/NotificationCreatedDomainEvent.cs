using Domain.Notifications;
using SharedKernel;

namespace Domain.Modules.Notifications;

public sealed record NotificationCreatedDomainEvent(
    Guid NotificationId,
    NotificationChannel Channel,
    NotificationPriority Priority,
    NotificationStatus Status) : IDomainEvent;
