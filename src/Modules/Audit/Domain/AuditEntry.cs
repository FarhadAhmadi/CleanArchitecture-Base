using SharedKernel;

namespace Domain.Auditing;

public sealed class AuditEntry : Entity
{
    public DateTime TimestampUtc { get; set; }
    public string ActorId { get; set; }
    public string Action { get; set; }
    public string ResourceType { get; set; }
    public string ResourceId { get; set; }
    public string PayloadHash { get; set; }
    public string PreviousChecksum { get; set; }
    public string Checksum { get; set; }
    public bool IsTampered { get; set; }
}
