namespace Application.Abstractions.Authorization;

public interface IPermissionCacheVersionService
{
    Task<long> GetVersionAsync(CancellationToken cancellationToken);
    Task BumpVersionAsync(CancellationToken cancellationToken);
}
