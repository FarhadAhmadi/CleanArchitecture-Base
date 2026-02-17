namespace Infrastructure.Integration;

internal sealed class InboxMessage
{
    public Guid Id { get; set; }
    public string MessageId { get; set; }
    public string Type { get; set; }
    public DateTime ReceivedOnUtc { get; set; }
    public DateTime? ProcessedOnUtc { get; set; }
    public string? Payload { get; set; }
    public string? Error { get; set; }
}
