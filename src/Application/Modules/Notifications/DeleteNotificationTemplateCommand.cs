using Application.Abstractions.Messaging;
using Application.Abstractions.Notifications;
using Domain.Notifications;

namespace Application.Notifications;

public sealed record DeleteNotificationTemplateCommand(Guid TemplateId) : ICommand<IResult>;
internal sealed class DeleteNotificationTemplateCommandHandler(INotificationUseCaseService service) : ResultWrappingCommandHandler<DeleteNotificationTemplateCommand>
{
    protected override async Task<IResult> HandleCore(DeleteNotificationTemplateCommand command, CancellationToken cancellationToken) =>
        await service.DeleteTemplateAsync(command.TemplateId, cancellationToken);
}





