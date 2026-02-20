using System.IO.Pipelines;
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

        var pipe = new Pipe();
        var producer = Task.Run(async () =>
        {
            await using Stream writerStream = pipe.Writer.AsStream(leaveOpen: true);
            try
            {
                GetObjectArgs getArgs = new GetObjectArgs()
                    .WithBucket(options.Bucket)
                    .WithObject(objectKey)
                    .WithCallbackStream(stream => stream.CopyTo(writerStream));

                await minioClient.GetObjectAsync(getArgs, cancellationToken);
                await pipe.Writer.CompleteAsync();
            }
            catch (Exception ex)
            {
                await pipe.Writer.CompleteAsync(ex);
            }
        }, CancellationToken.None);

        string contentType = string.IsNullOrWhiteSpace(stat.ContentType)
            ? "application/octet-stream"
            : stat.ContentType;

        Stream content = new PassthroughReadStream(pipe.Reader.AsStream(), producer);
        return (content, contentType);
    }

    private sealed class PassthroughReadStream(Stream inner, Task producer) : Stream
    {
        public override bool CanRead => inner.CanRead;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();

        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override void Flush() => inner.Flush();

        public override int Read(byte[] buffer, int offset, int count) => inner.Read(buffer, offset, count);

        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
            => inner.ReadAsync(buffer, cancellationToken);

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            => inner.ReadAsync(buffer, offset, count, cancellationToken);

        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

        public override void SetLength(long value) => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                inner.Dispose();
            }

            base.Dispose(disposing);
        }

        public override async ValueTask DisposeAsync()
        {
            await inner.DisposeAsync();
            await producer;
            await base.DisposeAsync();
        }
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
