using Domain.Logging;

namespace Application.Abstractions.Logging;

public sealed record ApplicationLogEntry(
    Guid Id,
    DateTime TimestampUtc,
    LogLevelType Level,
    string Message,
    string SourceService,
    string SourceModule,
    string TraceId,
    string ActorType,
    string? ActorId,
    string Outcome,
    string? TagsCsv = null,
    string? PayloadJson = null);

public interface IApplicationLogWriter
{
    Task<bool> TryWriteAsync(ApplicationLogEntry entry, CancellationToken cancellationToken);
}
