using SharedKernel;

namespace Domain.Files;

public sealed record FilePermissionUpsertedDomainEvent(
    Guid FileId,
    string SubjectType,
    string SubjectValue,
    bool CanRead,
    bool CanWrite,
    bool CanDelete) : IDomainEvent;
