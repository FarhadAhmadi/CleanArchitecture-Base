using SharedKernel;

namespace Domain.Modules.Notifications;

public sealed record NotificationArchivedDomainEvent(
    Guid NotificationId,
    DateTime ArchivedAtUtc) : IDomainEvent;
