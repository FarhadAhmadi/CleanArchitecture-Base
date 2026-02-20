using Application.Abstractions.Messaging;
using Application.Abstractions.Notifications;
using Domain.Notifications;

namespace Application.Notifications;

public sealed record ListNotificationsQuery(ListNotificationsRequest Request) : IQuery<IResult>;
internal sealed class ListNotificationsQueryHandler(INotificationUseCaseService service) : ResultWrappingQueryHandler<ListNotificationsQuery>
{
    protected override async Task<IResult> HandleCore(ListNotificationsQuery query, CancellationToken cancellationToken) =>
        await service.ListNotificationsAsync(query.Request, cancellationToken);
}





