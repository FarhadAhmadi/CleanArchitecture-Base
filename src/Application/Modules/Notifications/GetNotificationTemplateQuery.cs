using Application.Abstractions.Messaging;
using Application.Abstractions.Notifications;
using Domain.Notifications;

namespace Application.Notifications;

public sealed record GetNotificationTemplateQuery(Guid TemplateId) : IQuery<IResult>;
internal sealed class GetNotificationTemplateQueryHandler(INotificationUseCaseService service) : ResultWrappingQueryHandler<GetNotificationTemplateQuery>
{
    protected override async Task<IResult> HandleCore(GetNotificationTemplateQuery query, CancellationToken cancellationToken) =>
        await service.GetTemplateAsync(query.TemplateId, cancellationToken);
}





