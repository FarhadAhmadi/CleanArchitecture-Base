using Application.Abstractions.Messaging;
using Application.Abstractions.Notifications;
using Domain.Notifications;

namespace Application.Notifications;

public sealed record CreateNotificationCommand(CreateNotificationRequest Request) : ICommand<IResult>;
internal sealed class CreateNotificationCommandHandler(INotificationUseCaseService service) : ResultWrappingCommandHandler<CreateNotificationCommand>
{
    protected override async Task<IResult> HandleCore(CreateNotificationCommand command, CancellationToken cancellationToken) =>
        await service.CreateNotificationAsync(command.Request, cancellationToken);
}





