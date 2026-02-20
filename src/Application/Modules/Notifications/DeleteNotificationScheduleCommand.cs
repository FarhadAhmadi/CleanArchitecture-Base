using Application.Abstractions.Messaging;
using Application.Abstractions.Notifications;
using Domain.Notifications;

namespace Application.Notifications;

public sealed record DeleteNotificationScheduleCommand(Guid ScheduleId) : ICommand<IResult>;
internal sealed class DeleteNotificationScheduleCommandHandler(INotificationUseCaseService service) : ResultWrappingCommandHandler<DeleteNotificationScheduleCommand>
{
    protected override async Task<IResult> HandleCore(DeleteNotificationScheduleCommand command, CancellationToken cancellationToken) =>
        await service.DeleteScheduleAsync(command.ScheduleId, cancellationToken);
}





