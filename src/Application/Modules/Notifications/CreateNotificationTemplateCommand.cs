using Application.Abstractions.Messaging;
using Application.Abstractions.Notifications;
using Domain.Notifications;

namespace Application.Notifications;

public sealed record CreateNotificationTemplateCommand(CreateNotificationTemplateRequest Request) : ICommand<IResult>;
internal sealed class CreateNotificationTemplateCommandHandler(INotificationUseCaseService service) : ResultWrappingCommandHandler<CreateNotificationTemplateCommand>
{
    protected override async Task<IResult> HandleCore(CreateNotificationTemplateCommand command, CancellationToken cancellationToken) =>
        await service.CreateTemplateAsync(command.Request, cancellationToken);
}





