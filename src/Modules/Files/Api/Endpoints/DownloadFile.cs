using Application.Abstractions.Messaging;
using Web.Api.Endpoints.Users;
using Web.Api.Extensions;

using Application.Modules.Files;

namespace Web.Api.Endpoints.Files;

internal sealed class DownloadFile : IEndpoint, IOrderedEndpoint
{
    public int Order => 3;
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("files/{fileId:guid}/download", async (
                Guid fileId,
                HttpContext httpContext,
                IQueryHandler<DownloadFileQuery, IResult> handler,
                CancellationToken cancellationToken) =>
            (await handler.Handle(new DownloadFileQuery(fileId, httpContext), cancellationToken)).Match(static x => x, Web.Api.Infrastructure.CustomResults.Problem))
            .HasPermission(Permissions.FilesRead)
            .WithTags(Tags.Files);
    }
}






