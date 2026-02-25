namespace Infrastructure.Integration;

public sealed class IdempotencyRequest
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string ScopeHash { get; set; } = string.Empty;
    public string Scope { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime ExpiresAtUtc { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
    public int? StatusCode { get; set; }
    public string? ContentType { get; set; }
    public byte[]? ResponseBody { get; set; }
}
