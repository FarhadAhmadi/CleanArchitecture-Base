namespace Infrastructure.Integration;

public sealed class OutboxMessage
{
    public Guid Id { get; set; }
    public DateTime OccurredOnUtc { get; set; }
    public string Type { get; set; }
    public string Payload { get; set; }
    public DateTime? ProcessedOnUtc { get; set; }
    public int RetryCount { get; set; }
    public string? Error { get; set; }
}
