using SharedKernel;

namespace Domain.Users;

public sealed class UserExternalLogin : Entity
{
    public Guid UserId { get; set; }
    public string Provider { get; set; }
    public string ProviderUserId { get; set; }
    public string? Email { get; set; }
    public DateTime LinkedAtUtc { get; set; }
}
