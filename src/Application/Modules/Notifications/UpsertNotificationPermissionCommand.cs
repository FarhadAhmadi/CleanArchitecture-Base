using Application.Abstractions.Messaging;
using Application.Abstractions.Notifications;
using Domain.Notifications;

namespace Application.Notifications;

public sealed record UpsertNotificationPermissionCommand(
    Guid NotificationId,
    string SubjectType,
    string SubjectValue,
    bool CanRead,
    bool CanManage) : ICommand<IResult>;

internal sealed class UpsertNotificationPermissionCommandHandler(INotificationUseCaseService service) : ResultWrappingCommandHandler<UpsertNotificationPermissionCommand>
{
    protected override async Task<IResult> HandleCore(UpsertNotificationPermissionCommand command, CancellationToken cancellationToken) =>
        await service.UpsertPermissionAsync(
            command.NotificationId,
            command.SubjectType,
            command.SubjectValue,
            command.CanRead,
            command.CanManage,
            cancellationToken);
}





