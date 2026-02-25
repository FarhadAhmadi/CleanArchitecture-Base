using Application.Abstractions.Messaging;
using Web.Api.Endpoints.Users;
using Web.Api.Extensions;

using Application.Modules.Files;

namespace Web.Api.Endpoints.Files;

internal sealed class ShareFile : IEndpoint, IOrderedEndpoint
{
    public int Order => 6;

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("files/{fileId:guid}/share", async (
                Guid fileId,
                string? mode,
                HttpContext httpContext,
                ICommandHandler<ShareFileCommand, IResult> handler,
                CancellationToken cancellationToken) =>
            (await handler.Handle(new ShareFileCommand(fileId, mode, httpContext), cancellationToken)).Match(static x => x, Web.Api.Infrastructure.CustomResults.Problem))
            .HasPermission(Permissions.FilesShare)
            .WithTags(Tags.Files);
    }
}




