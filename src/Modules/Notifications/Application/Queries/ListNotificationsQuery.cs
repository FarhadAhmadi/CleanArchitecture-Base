using Application.Abstractions.Messaging;
using Application.Abstractions.Notifications;
using Domain.Notifications;

namespace Application.Notifications;

public sealed record ListNotificationsQuery(
    int? Page,
    int? PageIndex,
    int? PageSize,
    NotificationChannel? Channel,
    NotificationStatus? Status,
    DateTime? From,
    DateTime? To) : IQuery<IResult>;

internal sealed class ListNotificationsQueryHandler(INotificationUseCaseService service) : ResultWrappingQueryHandler<ListNotificationsQuery>
{
    protected override async Task<IResult> HandleCore(ListNotificationsQuery query, CancellationToken cancellationToken) =>
        await service.ListNotificationsAsync(
            query.Page,
            query.PageIndex,
            query.PageSize,
            query.Channel,
            query.Status,
            query.From,
            query.To,
            cancellationToken);
}





