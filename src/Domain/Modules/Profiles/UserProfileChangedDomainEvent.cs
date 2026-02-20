using SharedKernel;

namespace Domain.Profiles;

public sealed record UserProfileChangedDomainEvent(
    Guid ProfileId,
    Guid UserId,
    string ChangeType,
    int CompletenessScore) : IVersionedDomainEvent
{
    public string ContractName => "profiles.user-profile-changed";
    public int ContractVersion => 1;
}
