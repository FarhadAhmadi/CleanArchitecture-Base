using Application.Abstractions.Messaging;
using Application.Abstractions.Notifications;
using Domain.Notifications;

namespace Application.Notifications;

public sealed record CreateNotificationTemplateCommand(
    string Name,
    NotificationChannel Channel,
    string Language,
    string SubjectTemplate,
    string BodyTemplate) : ICommand<IResult>;

internal sealed class CreateNotificationTemplateCommandHandler(INotificationUseCaseService service) : ResultWrappingCommandHandler<CreateNotificationTemplateCommand>
{
    protected override async Task<IResult> HandleCore(CreateNotificationTemplateCommand command, CancellationToken cancellationToken) =>
        await service.CreateTemplateAsync(
            command.Name,
            command.Channel,
            command.Language,
            command.SubjectTemplate,
            command.BodyTemplate,
            cancellationToken);
}





