using Infrastructure.Files;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Minio;

namespace Infrastructure;

internal static class FilesModule
{
    internal static IServiceCollection AddFilesModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        FileStorageOptions storageOptions = configuration
            .GetSection(FileStorageOptions.SectionName)
            .Get<FileStorageOptions>() ?? new FileStorageOptions();

        FileValidationOptions validationOptions = configuration
            .GetSection(FileValidationOptions.SectionName)
            .Get<FileValidationOptions>() ?? new FileValidationOptions();

        ClamAvOptions clamAvOptions = configuration
            .GetSection(ClamAvOptions.SectionName)
            .Get<ClamAvOptions>() ?? new ClamAvOptions();

        services.AddSingleton(storageOptions);
        services.AddSingleton(validationOptions);
        services.AddSingleton(clamAvOptions);

        services.AddSingleton<IMinioClient>(_ =>
        {
            if (!storageOptions.Enabled)
            {
                throw new InvalidOperationException("FileStorage is disabled.");
            }

            if (string.IsNullOrWhiteSpace(storageOptions.AccessKey) ||
                string.IsNullOrWhiteSpace(storageOptions.SecretKey))
            {
                throw new InvalidOperationException("FileStorage credentials are required.");
            }

            IMinioClient clientBuilder = new MinioClient()
                .WithEndpoint(storageOptions.Endpoint)
                .WithCredentials(storageOptions.AccessKey, storageOptions.SecretKey);

            if (storageOptions.UseSsl)
            {
                clientBuilder = clientBuilder.WithSSL();
            }

            return clientBuilder.Build();
        });

        services.AddSingleton<IFileObjectStorage, MinioFileObjectStorage>();
        services.AddSingleton<FileAppLinkService>();

        if (clamAvOptions.Enabled)
        {
            services.AddSingleton<IFileMalwareScanner, ClamAvMalwareScanner>();
        }
        else
        {
            services.AddSingleton<IFileMalwareScanner, NoOpMalwareScanner>();
        }

        return services;
    }
}
