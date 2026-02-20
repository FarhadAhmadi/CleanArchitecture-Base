using Application.Abstractions.Messaging;
using Web.Api.Endpoints.Users;
using Web.Api.Extensions;

using Application.Modules.Files;

namespace Web.Api.Endpoints.Files;

internal sealed class FileLinks : IEndpoint, IOrderedEndpoint
{
    public int Order => 5;
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("files/{fileId:guid}/link", async (
                Guid fileId,
                string? mode,
                HttpContext httpContext,
                IQueryHandler<GetSecureFileLinkQuery, IResult> handler,
                CancellationToken cancellationToken) =>
            (await handler.Handle(new GetSecureFileLinkQuery(fileId, mode, httpContext), cancellationToken)).Match(static x => x, Web.Api.Infrastructure.CustomResults.Problem))
            .HasPermission(Permissions.FilesShare)
            .WithTags(Tags.Files);
    }
}






