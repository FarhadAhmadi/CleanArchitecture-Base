using Application.Abstractions.Files;
using Application.Abstractions.Messaging;
using Application.Modules.Files;
using Web.Api.Endpoints.Users;
using Web.Api.Extensions;

namespace Web.Api.Endpoints.Files;

public sealed record MoveFileRequest(string Module, string? Folder);

internal sealed class MoveFile : IEndpoint, IOrderedEndpoint
{
    public int Order => 9;

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPatch("files/{fileId:guid}/move", async (
                Guid fileId,
                MoveFileRequest request,
                ICommandHandler<MoveFileCommand, IResult> handler,
                CancellationToken cancellationToken) =>
            (await handler.Handle(
                new MoveFileCommand(fileId, new MoveFileInput(request.Module, request.Folder)),
                cancellationToken)).Match(static x => x, Web.Api.Infrastructure.CustomResults.Problem))
            .HasPermission(Permissions.FilesWrite)
            .WithTags(Tags.Files);
    }
}


