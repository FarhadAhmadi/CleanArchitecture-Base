using Application.Abstractions.Messaging;
using Application.Abstractions.Notifications;
using Domain.Notifications;

namespace Application.Notifications;

public sealed record GetNotificationQuery(Guid NotificationId) : IQuery<IResult>;
internal sealed class GetNotificationQueryHandler(INotificationUseCaseService service) : ResultWrappingQueryHandler<GetNotificationQuery>
{
    protected override async Task<IResult> HandleCore(GetNotificationQuery query, CancellationToken cancellationToken) =>
        await service.GetNotificationAsync(query.NotificationId, cancellationToken);
}





