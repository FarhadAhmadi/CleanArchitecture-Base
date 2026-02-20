using Application.Abstractions.Messaging;
using Application.Abstractions.Notifications;
using Domain.Notifications;

namespace Application.Notifications;

public sealed record ListNotificationSchedulesQuery() : IQuery<IResult>;
internal sealed class ListNotificationSchedulesQueryHandler(INotificationUseCaseService service) : ResultWrappingQueryHandler<ListNotificationSchedulesQuery>
{
    protected override async Task<IResult> HandleCore(ListNotificationSchedulesQuery query, CancellationToken cancellationToken) =>
        await service.ListSchedulesAsync(cancellationToken);
}





