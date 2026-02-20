using Application.Abstractions.Messaging;
using Web.Api.Endpoints.Users;
using Web.Api.Extensions;

using Application.Modules.Files;

namespace Web.Api.Endpoints.Files;

internal sealed class DeleteFile : IEndpoint, IOrderedEndpoint
{
    public int Order => 7;
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("files/{fileId:guid}", async (
                Guid fileId,
                HttpContext httpContext,
                ICommandHandler<DeleteFileCommand, IResult> handler,
                CancellationToken cancellationToken) =>
            (await handler.Handle(new DeleteFileCommand(fileId, httpContext), cancellationToken)).Match(static x => x, Web.Api.Infrastructure.CustomResults.Problem))
            .HasPermission(Permissions.FilesDelete)
            .WithTags(Tags.Files);
    }
}






