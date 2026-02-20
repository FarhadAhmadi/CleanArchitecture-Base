using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using SharedKernel;

namespace Domain.Users;

public sealed class User : IdentityUser<Guid>
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public User()
    {
        Id = Guid.NewGuid();
    }

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? AuditCreatedBy { get; set; }
    public DateTime AuditCreatedAtUtc { get; set; }
    public string? AuditUpdatedBy { get; set; }
    public DateTime? AuditUpdatedAtUtc { get; set; }

    [NotMapped]
    public int FailedLoginCount
    {
        get => AccessFailedCount;
        set => AccessFailedCount = value;
    }

    [NotMapped]
    public DateTime? LockoutEndUtc
    {
        get => LockoutEnd?.UtcDateTime;
        set => LockoutEnd = value.HasValue
            ? new DateTimeOffset(DateTime.SpecifyKind(value.Value, DateTimeKind.Utc))
            : null;
    }

    [NotMapped]
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
