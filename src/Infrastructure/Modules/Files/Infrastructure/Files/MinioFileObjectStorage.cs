using Minio;
using Minio.DataModel.Args;

namespace Infrastructure.Files;

internal sealed class MinioFileObjectStorage(
    IMinioClient minioClient,
    FileStorageOptions options) : IFileObjectStorage
{
    public async Task UploadAsync(
        string objectKey,
        Stream content,
        long contentLength,
        string contentType,
        CancellationToken cancellationToken)
    {
        await EnsureBucketAsync(cancellationToken);

        PutObjectArgs args = new PutObjectArgs()
            .WithBucket(options.Bucket)
            .WithObject(objectKey)
            .WithStreamData(content)
            .WithObjectSize(contentLength)
            .WithContentType(contentType);

        await minioClient.PutObjectAsync(args, cancellationToken);
    }

    public async Task DeleteAsync(string objectKey, CancellationToken cancellationToken)
    {
        RemoveObjectArgs args = new RemoveObjectArgs()
            .WithBucket(options.Bucket)
            .WithObject(objectKey);

        await minioClient.RemoveObjectAsync(args, cancellationToken);
    }

    public async Task<string> CreatePresignedDownloadUrlAsync(
        string objectKey,
        int expirySeconds,
        CancellationToken cancellationToken)
    {
        _ = cancellationToken;
        await EnsureBucketAsync(CancellationToken.None);

        PresignedGetObjectArgs args = new PresignedGetObjectArgs()
            .WithBucket(options.Bucket)
            .WithObject(objectKey)
            .WithExpiry(Math.Max(60, expirySeconds));

        return await minioClient.PresignedGetObjectAsync(args);
    }

    private async Task EnsureBucketAsync(CancellationToken cancellationToken)
    {
        if (!options.CreateBucketIfMissing)
        {
            return;
        }

        BucketExistsArgs existsArgs = new BucketExistsArgs().WithBucket(options.Bucket);
        bool exists = await minioClient.BucketExistsAsync(existsArgs, cancellationToken);
        if (exists)
        {
            return;
        }

        MakeBucketArgs makeArgs = new MakeBucketArgs().WithBucket(options.Bucket);
        await minioClient.MakeBucketAsync(makeArgs, cancellationToken);
    }
}
