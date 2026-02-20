using Application.Abstractions.Files;
using Application.Abstractions.Messaging;
using Application.Modules.Files;
using Web.Api.Endpoints.Users;
using Web.Api.Extensions;

namespace Web.Api.Endpoints.Files;

public sealed record UpdateFileMetadataRequest(string FileName, string? Description);

internal sealed class UpdateFileMetadata : IEndpoint, IOrderedEndpoint
{
    public int Order => 8;

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("files/{fileId:guid}", async (
                Guid fileId,
                UpdateFileMetadataRequest request,
                ICommandHandler<UpdateFileMetadataCommand, IResult> handler,
                CancellationToken cancellationToken) =>
            (await handler.Handle(
                new UpdateFileMetadataCommand(fileId, new UpdateFileMetadataInput(request.FileName, request.Description)),
                cancellationToken)).Match(static x => x, Web.Api.Infrastructure.CustomResults.Problem))
            .HasPermission(Permissions.FilesWrite)
            .WithTags(Tags.Files);
    }
}


