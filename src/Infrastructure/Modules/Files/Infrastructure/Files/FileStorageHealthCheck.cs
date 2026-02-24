using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Infrastructure.Files;

internal sealed class FileStorageHealthCheck(IFileObjectStorage storage) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await storage.MarkHealthyAsync(cancellationToken);
            return HealthCheckResult.Healthy("File object storage is reachable.");
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy("File object storage is not reachable.", exception);
        }
    }
}
