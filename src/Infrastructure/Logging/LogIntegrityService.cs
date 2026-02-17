using System.Security.Cryptography;
using System.Text;
using Domain.Logging;

namespace Infrastructure.Logging;

internal sealed class LogIntegrityService : ILogIntegrityService
{
    public string ComputeChecksum(LogEvent logEvent)
    {
        string canonical = string.Join('|',
            logEvent.Id,
            logEvent.TimestampUtc.ToString("O"),
            logEvent.Level,
            logEvent.Message,
            logEvent.SourceService,
            logEvent.SourceModule,
            logEvent.TraceId,
            logEvent.RequestId ?? string.Empty,
            logEvent.ActorType,
            logEvent.ActorId ?? string.Empty,
            logEvent.Outcome,
            logEvent.ErrorCode ?? string.Empty,
            logEvent.ErrorStackHash ?? string.Empty,
            logEvent.TagsCsv ?? string.Empty,
            logEvent.PayloadJson ?? string.Empty);

        byte[] bytes = Encoding.UTF8.GetBytes(canonical);
        byte[] hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
    }

    public bool IsCorrupted(LogEvent logEvent)
    {
        string recomputed = ComputeChecksum(logEvent);
        return !string.Equals(recomputed, logEvent.Checksum, StringComparison.OrdinalIgnoreCase);
    }
}
