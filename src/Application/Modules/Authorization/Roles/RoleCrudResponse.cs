namespace Application.Authorization.Roles;

public sealed class RoleCrudResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
