namespace Infrastructure.Logging;

public interface ILogIngestionService
{
    Task<IngestResult> IngestAsync(IngestLogRequest request, string? idempotencyKey, CancellationToken cancellationToken);
}
