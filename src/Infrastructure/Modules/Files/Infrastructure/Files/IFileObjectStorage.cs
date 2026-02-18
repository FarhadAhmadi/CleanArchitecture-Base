namespace Infrastructure.Files;

public interface IFileObjectStorage
{
    Task UploadAsync(
        string objectKey,
        Stream content,
        long contentLength,
        string contentType,
        CancellationToken cancellationToken);

    Task DeleteAsync(string objectKey, CancellationToken cancellationToken);

    Task<string> CreatePresignedDownloadUrlAsync(
        string objectKey,
        int expirySeconds,
        CancellationToken cancellationToken);
}
