using Application.Abstractions.Messaging;
using Web.Api.Endpoints.Users;
using Web.Api.Extensions;

using Application.Modules.Files;

namespace Web.Api.Endpoints.Files;

internal sealed class StreamFile : IEndpoint, IOrderedEndpoint
{
    public int Order => 4;
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("files/{fileId:guid}/stream", async (
                Guid fileId,
                HttpContext httpContext,
                IQueryHandler<StreamFileQuery, IResult> handler,
                CancellationToken cancellationToken) =>
            (await handler.Handle(new StreamFileQuery(fileId, httpContext), cancellationToken)).Match(static x => x, Web.Api.Infrastructure.CustomResults.Problem))
            .HasPermission(Permissions.FilesRead)
            .WithTags(Tags.Files);
    }
}






