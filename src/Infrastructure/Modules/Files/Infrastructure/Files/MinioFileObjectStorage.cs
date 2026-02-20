using Minio;
using Minio.DataModel;
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

    public async Task<(Stream Content, string ContentType)> OpenReadAsync(
        string objectKey,
        CancellationToken cancellationToken)
    {
        await EnsureBucketAsync(cancellationToken);

        StatObjectArgs statArgs = new StatObjectArgs()
            .WithBucket(options.Bucket)
            .WithObject(objectKey);

        ObjectStat stat = await minioClient.StatObjectAsync(statArgs, cancellationToken);

        var content = new MemoryStream();
        GetObjectArgs getArgs = new GetObjectArgs()
            .WithBucket(options.Bucket)
            .WithObject(objectKey)
            .WithCallbackStream(stream => stream.CopyTo(content));

        await minioClient.GetObjectAsync(getArgs, cancellationToken);
        content.Position = 0;

        string contentType = string.IsNullOrWhiteSpace(stat.ContentType)
            ? "application/octet-stream"
            : stat.ContentType;

        return (content, contentType);
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
