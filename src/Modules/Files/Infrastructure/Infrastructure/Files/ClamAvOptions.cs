namespace Infrastructure.Files;

public sealed class ClamAvOptions
{
    public const string SectionName = "ClamAv";

    public bool Enabled { get; init; }
    public string Host { get; init; } = "localhost";
    public int Port { get; init; } = 3310;
    public int TimeoutSeconds { get; init; } = 20;
}
