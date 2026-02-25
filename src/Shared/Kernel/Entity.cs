namespace SharedKernel;

public abstract class Entity
{
    private readonly List<IDomainEvent> _domainEvents = [];
    public Guid Id { get; set; } = Guid.NewGuid();
    public string? AuditCreatedBy { get; set; }
    public DateTime AuditCreatedAtUtc { get; set; }
    public string? AuditUpdatedBy { get; set; }
    public DateTime? AuditUpdatedAtUtc { get; set; }

    public List<IDomainEvent> DomainEvents => [.. _domainEvents];

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    public void Raise(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }
}
