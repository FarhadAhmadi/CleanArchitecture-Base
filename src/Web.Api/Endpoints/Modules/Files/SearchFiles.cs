using Microsoft.AspNetCore.Mvc;
using Application.Abstractions.Files;
using Application.Abstractions.Messaging;
using Application.Modules.Files;
using Web.Api.Endpoints.Common.Requests;
using Web.Api.Endpoints.Users;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Files;

public sealed class SearchFilesRequest : PagedSortedQueryRequest
{
    [FromQuery(Name = "query")]
    [SanitizeText(200)]
    public string? Query { get; set; }

    [FromQuery(Name = "fileType")]
    [SanitizeIdentifier(20)]
    public string? FileType { get; set; }

    [FromQuery(Name = "uploaderId")]
    public Guid? UploaderId { get; set; }

    [FromQuery(Name = "from")]
    public DateTime? From { get; set; }

    [FromQuery(Name = "to")]
    public DateTime? To { get; set; }
}

internal sealed class SearchFiles : IEndpoint, IOrderedEndpoint
{
    public int Order => 10;

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("files/search", async (
                [AsParameters] SearchFilesRequest request,
                IQueryHandler<SearchFilesQuery, IResult> handler,
                CancellationToken cancellationToken) =>
            {
                (int page, int pageSize) = request.NormalizePaging();

                return (await handler.Handle(
                    new SearchFilesQuery(
                        new SearchFilesInput(request.Query, request.FileType, request.UploaderId, request.From, request.To, page, pageSize)),
                    cancellationToken)).Match(static x => x, Web.Api.Infrastructure.CustomResults.Problem);
            })
            .HasPermission(Permissions.FilesRead)
            .WithTags(Tags.Files);
    }
}


