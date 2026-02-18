using System.Security.Cryptography;
using System.Text;
using Domain.Auditing;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Auditing;

internal sealed class AuditTrailService(ApplicationDbContext dbContext) : IAuditTrailService
{
    public async Task RecordAsync(AuditRecordRequest request, CancellationToken cancellationToken)
    {
        string payloadHash = ComputeHash(request.PayloadJson);

        string previousChecksum = await dbContext.AuditEntries
            .OrderByDescending(x => x.TimestampUtc)
            .ThenByDescending(x => x.Id)
            .Select(x => x.Checksum)
            .FirstOrDefaultAsync(cancellationToken) ?? "GENESIS";

        AuditEntry entry = new()
        {
            Id = Guid.NewGuid(),
            TimestampUtc = DateTime.UtcNow,
            ActorId = request.ActorId,
            Action = request.Action,
            ResourceType = request.ResourceType,
            ResourceId = request.ResourceId,
            PayloadHash = payloadHash,
            PreviousChecksum = previousChecksum,
            Checksum = ComputeChecksum(
                request.ActorId,
                request.Action,
                request.ResourceType,
                request.ResourceId,
                payloadHash,
                previousChecksum),
            IsTampered = false
        };

        dbContext.AuditEntries.Add(entry);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<bool> IsTamperedAsync(AuditEntry entry, CancellationToken cancellationToken)
    {
        string recomputed = ComputeChecksum(
            entry.ActorId,
            entry.Action,
            entry.ResourceType,
            entry.ResourceId,
            entry.PayloadHash,
            entry.PreviousChecksum);

        bool isTampered = !string.Equals(recomputed, entry.Checksum, StringComparison.OrdinalIgnoreCase);
        return Task.FromResult(isTampered);
    }

    private static string ComputeHash(string payload)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(payload);
        byte[] hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
    }

    private static string ComputeChecksum(
        string actorId,
        string action,
        string resourceType,
        string resourceId,
        string payloadHash,
        string previousChecksum)
    {
        string canonical = string.Join('|', actorId, action, resourceType, resourceId, payloadHash, previousChecksum);
        byte[] bytes = Encoding.UTF8.GetBytes(canonical);
        byte[] hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
    }
}
