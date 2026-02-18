using SharedKernel;

namespace Domain.Authorization;

public sealed class Permission : Entity
{
    public string Code { get; set; }
    public string Description { get; set; }
}
