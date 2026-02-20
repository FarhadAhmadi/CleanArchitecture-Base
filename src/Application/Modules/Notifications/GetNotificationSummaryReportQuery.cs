using Application.Abstractions.Messaging;
using Application.Abstractions.Notifications;
using Domain.Notifications;

namespace Application.Notifications;

public sealed record GetNotificationSummaryReportQuery(DateTime? From, DateTime? To) : IQuery<IResult>;
internal sealed class GetNotificationSummaryReportQueryHandler(INotificationUseCaseService service) : ResultWrappingQueryHandler<GetNotificationSummaryReportQuery>
{
    protected override async Task<IResult> HandleCore(GetNotificationSummaryReportQuery query, CancellationToken cancellationToken) =>
        await service.ReportSummaryAsync(query.From, query.To, cancellationToken);
}





