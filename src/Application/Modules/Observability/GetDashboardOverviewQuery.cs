using Application.Abstractions.Messaging;
using Application.Abstractions.Observability;

namespace Application.Observability;

public sealed record GetDashboardOverviewQuery() : IQuery<IResult>;

internal sealed class GetDashboardOverviewQueryHandler(
    IObservabilityDashboardService dashboardService) : ResultWrappingQueryHandler<GetDashboardOverviewQuery>
{
    protected override async Task<IResult> HandleCore(GetDashboardOverviewQuery query, CancellationToken cancellationToken)
    {
        _ = query;
        ObservabilityDashboardSnapshot snapshot = await dashboardService.GetSnapshotAsync(cancellationToken);
        return Results.Ok(snapshot);
    }
}
