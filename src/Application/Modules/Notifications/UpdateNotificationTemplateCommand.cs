using Application.Abstractions.Messaging;
using Application.Abstractions.Notifications;
using Domain.Notifications;

namespace Application.Notifications;

public sealed record UpdateNotificationTemplateCommand(Guid TemplateId, string SubjectTemplate, string BodyTemplate) : ICommand<IResult>;
internal sealed class UpdateNotificationTemplateCommandHandler(INotificationUseCaseService service) : ResultWrappingCommandHandler<UpdateNotificationTemplateCommand>
{
    protected override async Task<IResult> HandleCore(UpdateNotificationTemplateCommand command, CancellationToken cancellationToken) =>
        await service.UpdateTemplateAsync(command.TemplateId, command.SubjectTemplate, command.BodyTemplate, cancellationToken);
}





