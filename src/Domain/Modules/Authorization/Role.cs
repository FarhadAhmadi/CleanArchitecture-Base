using SharedKernel;

namespace Domain.Authorization;

public sealed class Role : Entity
{
    public string Name { get; set; }
}
