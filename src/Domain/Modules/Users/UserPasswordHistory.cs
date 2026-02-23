using SharedKernel;

namespace Domain.Users;

public sealed class UserPasswordHistory : Entity
{
    public Guid UserId { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
}
