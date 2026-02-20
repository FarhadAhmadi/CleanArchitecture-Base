using Application.Abstractions.Messaging;
using Application.Abstractions.Notifications;
using Domain.Notifications;

namespace Application.Notifications;

public sealed record ScheduleNotificationCommand(Guid NotificationId, ScheduleNotificationRequest Request) : ICommand<IResult>;
internal sealed class ScheduleNotificationCommandHandler(INotificationUseCaseService service) : ResultWrappingCommandHandler<ScheduleNotificationCommand>
{
    protected override async Task<IResult> HandleCore(ScheduleNotificationCommand command, CancellationToken cancellationToken) =>
        await service.ScheduleNotificationAsync(command.NotificationId, command.Request, cancellationToken);
}





