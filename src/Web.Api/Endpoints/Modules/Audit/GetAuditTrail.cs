using Application.Abstractions.Messaging;
using Application.Audit;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Endpoints.Common.Requests;
using Web.Api.Endpoints.Users;
using Web.Api.Extensions;

namespace Web.Api.Endpoints.Audit;

public sealed class GetAuditEntriesRequest : PagedSortedQueryRequest
{
    public string? ActorId { get; set; }
    public string? Action { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
}

internal sealed class GetAuditTrail : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("audit", async (
                [AsParameters] GetAuditEntriesRequest request,
                IQueryHandler<GetAuditEntriesQuery, IResult> handler,
                CancellationToken cancellationToken) =>
            (await handler.Handle(
                new GetAuditEntriesQuery(
                    request.Page,
                    request.PageIndex,
                    request.PageSize,
                    request.ActorId,
                    request.Action,
                    request.From,
                    request.To),
                cancellationToken)).Match(static x => x, Web.Api.Infrastructure.CustomResults.Problem))
            .WithTags("Audit")
            .HasPermission(Permissions.AuditRead);
    }
}

