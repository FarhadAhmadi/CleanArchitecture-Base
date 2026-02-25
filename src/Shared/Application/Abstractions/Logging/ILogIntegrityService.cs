using Domain.Logging;

namespace Application.Abstractions.Logging;

public interface ILogIntegrityService
{
    string ComputeChecksum(LogEvent logEvent);
    bool IsCorrupted(LogEvent logEvent);
}
