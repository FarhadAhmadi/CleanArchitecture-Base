using Domain.Auditing;

namespace Application.Abstractions.Auditing;

public interface IAuditTrailService
{
    Task RecordAsync(AuditRecordRequest request, CancellationToken cancellationToken);
    Task<bool> IsTamperedAsync(AuditEntry entry, CancellationToken cancellationToken);
}
