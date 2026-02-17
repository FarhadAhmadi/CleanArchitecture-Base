namespace Infrastructure.Integration;

internal interface IInboxStore
{
    Task<bool> TryStartAsync(string messageId, string messageType, string? payload, CancellationToken cancellationToken);
    Task MarkProcessedAsync(string messageId, CancellationToken cancellationToken);
    Task MarkFailedAsync(string messageId, string error, CancellationToken cancellationToken);
}
