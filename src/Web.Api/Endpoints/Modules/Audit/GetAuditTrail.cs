using Application.Abstractions.Messaging;
using Application.Audit;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Endpoints.Users;
using Web.Api.Extensions;

namespace Web.Api.Endpoints.Audit;

internal sealed class GetAuditTrail : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("audit", async (
                [AsParameters] GetAuditEntriesRequest request,
                IQueryHandler<GetAuditEntriesQuery, IResult> handler,
                CancellationToken cancellationToken) =>
            (await handler.Handle(new GetAuditEntriesQuery(request), cancellationToken)).Match(static x => x, Web.Api.Infrastructure.CustomResults.Problem))
            .WithTags("Audit")
            .HasPermission(Permissions.AuditRead);
    }
}

