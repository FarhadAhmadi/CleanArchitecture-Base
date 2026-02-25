using Application.Abstractions.Messaging;
using Web.Api.Endpoints.Users;
using Web.Api.Extensions;

using Application.Modules.Files;

namespace Web.Api.Endpoints.Files;

internal sealed class FileEncryption : IEndpoint, IOrderedEndpoint
{
    public int Order => 14;
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("files/encrypt/{fileId:guid}", async (
                Guid fileId,
                ICommandHandler<SetFileEncryptedCommand, IResult> handler,
                CancellationToken cancellationToken) =>
            (await handler.Handle(new SetFileEncryptedCommand(fileId), cancellationToken)).Match(static x => x, Web.Api.Infrastructure.CustomResults.Problem))
            .HasPermission(Permissions.FilesPermissionsManage)
            .WithTags(Tags.Files);
    }
}






