namespace Infrastructure.Authorization;

public sealed class AuthorizationBootstrapOptions
{
    public const string SectionName = "AuthorizationBootstrap";

    public bool SeedDefaults { get; init; } = true;

    public string UserRoleName { get; init; } = "user";

    public string AdminRoleName { get; init; } = "admin";

    public string[] AdminEmails { get; init; } = [];
}
