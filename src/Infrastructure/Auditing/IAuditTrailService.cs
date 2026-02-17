using Domain.Auditing;

namespace Infrastructure.Auditing;

public interface IAuditTrailService
{
    Task RecordAsync(AuditRecordRequest request, CancellationToken cancellationToken);
    Task<bool> IsTamperedAsync(AuditEntry entry, CancellationToken cancellationToken);
}
