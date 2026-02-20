using Application.Abstractions.Messaging;
using Application.Abstractions.Notifications;
using Domain.Notifications;

namespace Application.Notifications;

public sealed record CreateNotificationCommand(
    NotificationChannel Channel,
    NotificationPriority Priority,
    string Recipient,
    string? Subject,
    string? Body,
    string Language,
    Guid? TemplateId,
    DateTime? ScheduledAtUtc) : ICommand<IResult>;

internal sealed class CreateNotificationCommandHandler(INotificationUseCaseService service) : ResultWrappingCommandHandler<CreateNotificationCommand>
{
    protected override async Task<IResult> HandleCore(CreateNotificationCommand command, CancellationToken cancellationToken) =>
        await service.CreateNotificationAsync(
            command.Channel,
            command.Priority,
            command.Recipient,
            command.Subject,
            command.Body,
            command.Language,
            command.TemplateId,
            command.ScheduledAtUtc,
            cancellationToken);
}





