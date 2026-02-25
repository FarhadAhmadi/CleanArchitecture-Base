using Microsoft.AspNetCore.Mvc;
using Application.Abstractions.Files;
using Application.Abstractions.Messaging;
using Application.Modules.Files;
using Web.Api.Endpoints.Common.Requests;
using Web.Api.Endpoints.Users;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Files;

public sealed class FilterFilesRequest : PagedSortedQueryRequest
{
    [FromQuery(Name = "module")]
    [SanitizeIdentifier(100)]
    public string? Module { get; set; }
}

internal sealed class FilterFiles : IEndpoint, IOrderedEndpoint
{
    public int Order => 11;

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("files/filter", async (
                [AsParameters] FilterFilesRequest request,
                IQueryHandler<FilterFilesQuery, IResult> handler,
                CancellationToken cancellationToken) =>
            {
                (int page, int pageSize) = request.NormalizePaging();

                return (await handler.Handle(
                    new FilterFilesQuery(new FilterFilesInput(request.Module, page, pageSize)),
                    cancellationToken)).Match(static x => x, Web.Api.Infrastructure.CustomResults.Problem);
            })
            .HasPermission(Permissions.FilesRead)
            .WithTags(Tags.Files);
    }
}


