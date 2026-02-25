namespace Infrastructure.Authorization;

public sealed class PermissionCacheOptions
{
    public const string SectionName = "PermissionCache";

    public int AbsoluteExpirationSeconds { get; init; } = 300;
}
