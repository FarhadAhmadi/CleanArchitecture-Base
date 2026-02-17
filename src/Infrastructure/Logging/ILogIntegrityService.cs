using Domain.Logging;

namespace Infrastructure.Logging;

public interface ILogIntegrityService
{
    string ComputeChecksum(LogEvent logEvent);
    bool IsCorrupted(LogEvent logEvent);
}
