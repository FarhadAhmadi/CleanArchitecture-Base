namespace Infrastructure.Auditing;

public sealed record AuditRecordRequest(
    string ActorId,
    string Action,
    string ResourceType,
    string ResourceId,
    string PayloadJson);
