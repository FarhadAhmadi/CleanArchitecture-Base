using SharedKernel;

namespace Domain.Files;

public sealed record FileUploadedDomainEvent(
    Guid FileId,
    Guid OwnerUserId,
    string Module,
    long SizeBytes) : IDomainEvent;
