using SharedKernel;

namespace Domain.Users;

public sealed record UserRegisteredDomainEvent(Guid UserId) : IVersionedDomainEvent
{
    public string ContractName => "users.user-registered";
    public int ContractVersion => 1;
}
