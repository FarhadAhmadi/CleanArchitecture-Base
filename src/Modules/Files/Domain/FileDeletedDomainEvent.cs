using SharedKernel;

namespace Domain.Files;

public sealed record FileDeletedDomainEvent(
    Guid FileId,
    Guid OwnerUserId) : IDomainEvent;
