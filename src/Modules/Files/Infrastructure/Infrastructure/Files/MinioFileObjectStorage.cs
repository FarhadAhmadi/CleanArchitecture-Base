using System.IO.Pipelines;
using System.Diagnostics;
using Minio;
using Minio.DataModel;
using Minio.DataModel.Args;
using Minio.Exceptions;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;

namespace Infrastructure.Files;

internal sealed class MinioFileObjectStorage(
    IMinioClient minioClient,
    FileStorageOptions options,
    FileStorageMetrics metrics) : IFileObjectStorage
{
    private readonly ResiliencePipeline _pipeline = BuildPipeline(options);

    public async Task UploadAsync(
        string objectKey,
        Stream content,
        long contentLength,
        string contentType,
        CancellationToken cancellationToken)
    {
        await ExecuteWithMetricsAsync("upload", async token =>
        {
            await EnsureBucketAsync(token);

            PutObjectArgs args = new PutObjectArgs()
                .WithBucket(options.Bucket)
                .WithObject(objectKey)
                .WithStreamData(content)
                .WithObjectSize(contentLength)
                .WithContentType(contentType);

            await minioClient.PutObjectAsync(args, token);
        }, cancellationToken);
    }

    public async Task DeleteAsync(string objectKey, CancellationToken cancellationToken)
    {
        await ExecuteWithMetricsAsync("delete", async token =>
        {
            RemoveObjectArgs args = new RemoveObjectArgs()
                .WithBucket(options.Bucket)
                .WithObject(objectKey);

            await minioClient.RemoveObjectAsync(args, token);
        }, cancellationToken);
    }

    public async Task<string> CreatePresignedDownloadUrlAsync(
        string objectKey,
        int expirySeconds,
        CancellationToken cancellationToken)
    {
        return await ExecuteWithMetricsAsync("presign", async token =>
        {
            await EnsureBucketAsync(token);

            PresignedGetObjectArgs args = new PresignedGetObjectArgs()
                .WithBucket(options.Bucket)
                .WithObject(objectKey)
                .WithExpiry(Math.Max(60, expirySeconds));

            return await minioClient.PresignedGetObjectAsync(args);
        }, cancellationToken);
    }

    public async Task<(Stream Content, string ContentType)> OpenReadAsync(
        string objectKey,
        CancellationToken cancellationToken)
    {
        await EnsureBucketAsync(cancellationToken);

        ObjectStat stat = await ExecuteWithMetricsAsync("stat", async token =>
        {
            StatObjectArgs statArgs = new StatObjectArgs()
                .WithBucket(options.Bucket)
                .WithObject(objectKey);

            return await minioClient.StatObjectAsync(statArgs, token);
        }, cancellationToken);

        var pipe = new Pipe();
        var producer = Task.Run(async () =>
        {
            await using Stream writerStream = pipe.Writer.AsStream(leaveOpen: true);
            try
            {
                await ExecuteWithMetricsAsync("read", async token =>
                {
                    GetObjectArgs getArgs = new GetObjectArgs()
                        .WithBucket(options.Bucket)
                        .WithObject(objectKey)
                        .WithCallbackStream(stream => stream.CopyTo(writerStream));

                    await minioClient.GetObjectAsync(getArgs, token);
                }, cancellationToken);

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

    public async Task<bool> ExistsAsync(string objectKey, CancellationToken cancellationToken)
    {
        try
        {
            await ExecuteWithMetricsAsync("exists", async token =>
            {
                StatObjectArgs statArgs = new StatObjectArgs()
                    .WithBucket(options.Bucket)
                    .WithObject(objectKey);

                _ = await minioClient.StatObjectAsync(statArgs, token);
            }, cancellationToken);

            return true;
        }
        catch (FileNotFoundException)
        {
            return false;
        }
    }

    public async Task MarkHealthyAsync(CancellationToken cancellationToken)
    {
        await ExecuteWithMetricsAsync("health", EnsureBucketAsync, cancellationToken);
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
        BucketExistsArgs existsArgs = new BucketExistsArgs().WithBucket(options.Bucket);
        bool exists = await minioClient.BucketExistsAsync(existsArgs, cancellationToken);
        if (!exists && options.CreateBucketIfMissing)
        {
            MakeBucketArgs makeArgs = new MakeBucketArgs().WithBucket(options.Bucket);
            await minioClient.MakeBucketAsync(makeArgs, cancellationToken);
        }
    }

    private async Task ExecuteWithMetricsAsync(string operation, Func<CancellationToken, Task> action, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            await _pipeline.ExecuteAsync(async token =>
            {
                await action(token);
            }, cancellationToken);
            metrics.RecordOperation(operation, "success", stopwatch.Elapsed.TotalMilliseconds);
        }
        catch (Exception exception) when (TryConvertNotFound(operation, exception, stopwatch.Elapsed.TotalMilliseconds, out Exception converted))
        {
            throw converted;
        }
        catch
        {
            metrics.RecordOperation(operation, "failure", stopwatch.Elapsed.TotalMilliseconds);
            throw;
        }
    }

    private async Task<T> ExecuteWithMetricsAsync<T>(string operation, Func<CancellationToken, Task<T>> action, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            T? result = default;
            await _pipeline.ExecuteAsync(async token =>
            {
                result = await action(token);
            }, cancellationToken);
            metrics.RecordOperation(operation, "success", stopwatch.Elapsed.TotalMilliseconds);
            return result!;
        }
        catch (Exception exception) when (TryConvertNotFound(operation, exception, stopwatch.Elapsed.TotalMilliseconds, out Exception converted))
        {
            throw converted;
        }
        catch
        {
            metrics.RecordOperation(operation, "failure", stopwatch.Elapsed.TotalMilliseconds);
            throw;
        }
    }

    private bool TryConvertNotFound(string operation, Exception exception, double elapsedMs, out Exception converted)
    {
        if (IsObjectNotFound(exception))
        {
            metrics.RecordOperation(operation, "not_found", elapsedMs);
            metrics.RecordObjectNotFound(operation);
            converted = new FileNotFoundException("Object not found in storage.", exception);
            return true;
        }

        converted = exception;
        return false;
    }

    private static bool IsObjectNotFound(Exception exception)
    {
        return exception is ObjectNotFoundException ||
               exception.InnerException is ObjectNotFoundException;
    }

    private static ResiliencePipeline BuildPipeline(FileStorageOptions settings)
    {
        return new ResiliencePipelineBuilder()
            .AddTimeout(new TimeoutStrategyOptions
            {
                Timeout = TimeSpan.FromSeconds(Math.Max(1, settings.RequestTimeoutSeconds))
            })
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = Math.Max(1, settings.RetryMaxAttempts),
                BackoffType = DelayBackoffType.Exponential,
                Delay = TimeSpan.FromMilliseconds(Math.Max(50, settings.RetryBaseDelayMs)),
                ShouldHandle = new PredicateBuilder()
                    .Handle<Exception>(exception => exception is not FileNotFoundException && !IsObjectNotFound(exception))
            })
            .AddCircuitBreaker(new CircuitBreakerStrategyOptions
            {
                FailureRatio = Math.Clamp(settings.CircuitBreakerFailureRatioPercent / 100d, 0.05, 0.95),
                SamplingDuration = TimeSpan.FromSeconds(Math.Max(5, settings.CircuitBreakerSamplingSeconds)),
                MinimumThroughput = Math.Max(2, settings.CircuitBreakerMinimumThroughput),
                BreakDuration = TimeSpan.FromSeconds(Math.Max(5, settings.CircuitBreakerBreakSeconds)),
                ShouldHandle = new PredicateBuilder()
                    .Handle<Exception>(exception => exception is not FileNotFoundException && !IsObjectNotFound(exception))
            })
            .Build();
    }
}
