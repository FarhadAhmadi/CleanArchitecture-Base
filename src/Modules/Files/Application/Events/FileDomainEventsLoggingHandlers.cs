using Domain.Files;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace Application.Modules.Files;

internal sealed class FileUploadedDomainEventHandler(
    ILogger<FileUploadedDomainEventHandler> logger)
    : IDomainEventHandler<FileUploadedDomainEvent>
{
    public Task Handle(FileUploadedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "File uploaded domain event. FileId={FileId} OwnerUserId={OwnerUserId} Module={Module} SizeBytes={SizeBytes}",
                domainEvent.FileId,
                domainEvent.OwnerUserId,
                domainEvent.Module,
                domainEvent.SizeBytes);
        }

        return Task.CompletedTask;
    }
}

internal sealed class FileDeletedDomainEventHandler(
    ILogger<FileDeletedDomainEventHandler> logger)
    : IDomainEventHandler<FileDeletedDomainEvent>
{
    public Task Handle(FileDeletedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "File deleted domain event. FileId={FileId} OwnerUserId={OwnerUserId}",
                domainEvent.FileId,
                domainEvent.OwnerUserId);
        }

        return Task.CompletedTask;
    }
}

internal sealed class FilePermissionUpsertedDomainEventHandler(
    ILogger<FilePermissionUpsertedDomainEventHandler> logger)
    : IDomainEventHandler<FilePermissionUpsertedDomainEvent>
{
    public Task Handle(FilePermissionUpsertedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "File permission upserted domain event. FileId={FileId} SubjectType={SubjectType} SubjectValue={SubjectValue} CanRead={CanRead} CanWrite={CanWrite} CanDelete={CanDelete}",
                domainEvent.FileId,
                domainEvent.SubjectType,
                domainEvent.SubjectValue,
                domainEvent.CanRead,
                domainEvent.CanWrite,
                domainEvent.CanDelete);
        }

        return Task.CompletedTask;
    }
}
