namespace Infrastructure.Caching;

public sealed class RedisCacheOptions
{
    public const string SectionName = "RedisCache";

    public bool Enabled { get; init; }
    public string? ConnectionString { get; init; }
    public string InstanceName { get; init; } = "clean-architecture:";
}
