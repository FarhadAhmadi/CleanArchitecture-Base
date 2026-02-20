using Application.Abstractions.Messaging;
using Application.Abstractions.Notifications;
using Domain.Notifications;

namespace Application.Notifications;

public sealed record ListNotificationTemplatesQuery(string? Language, NotificationChannel? Channel) : IQuery<IResult>;
internal sealed class ListNotificationTemplatesQueryHandler(INotificationUseCaseService service) : ResultWrappingQueryHandler<ListNotificationTemplatesQuery>
{
    protected override async Task<IResult> HandleCore(ListNotificationTemplatesQuery query, CancellationToken cancellationToken) =>
        await service.ListTemplatesAsync(query.Language, query.Channel, cancellationToken);
}





