using Application.Abstractions.Messaging;
using Application.Audit;
using Web.Api.Endpoints.Users;
using Web.Api.Extensions;

namespace Web.Api.Endpoints.Audit;

internal sealed class GetAuditIntegrityReport : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("audit/integrity", async (
                bool updateFlags,
                IQueryHandler<GetAuditIntegrityReportQuery, IResult> handler,
                CancellationToken cancellationToken) =>
            (await handler.Handle(new GetAuditIntegrityReportQuery(updateFlags), cancellationToken)).Match(static x => x, Web.Api.Infrastructure.CustomResults.Problem))
            .WithTags("Audit")
            .HasPermission(Permissions.AuditRead);
    }
}

