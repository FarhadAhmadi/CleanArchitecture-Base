using Application.Abstractions.Messaging;
using Application.Abstractions.Notifications;
using Domain.Notifications;

namespace Application.Notifications;

public sealed record ArchiveNotificationCommand(Guid NotificationId) : ICommand<IResult>;
internal sealed class ArchiveNotificationCommandHandler(INotificationUseCaseService service) : ResultWrappingCommandHandler<ArchiveNotificationCommand>
{
    protected override async Task<IResult> HandleCore(ArchiveNotificationCommand command, CancellationToken cancellationToken) =>
        await service.ArchiveAsync(command.NotificationId, cancellationToken);
}





