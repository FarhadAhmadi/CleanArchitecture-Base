using Application.Abstractions.Messaging;
using Web.Api.Endpoints.Users;
using Web.Api.Extensions;

using Application.Modules.Files;

namespace Web.Api.Endpoints.Files;

internal sealed class SetFileDecrypted : IEndpoint, IOrderedEndpoint
{
    public int Order => 15;

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("files/decrypt/{fileId:guid}", async (
                Guid fileId,
                ICommandHandler<SetFileDecryptedCommand, IResult> handler,
                CancellationToken cancellationToken) =>
            (await handler.Handle(new SetFileDecryptedCommand(fileId), cancellationToken)).Match(static x => x, Web.Api.Infrastructure.CustomResults.Problem))
            .HasPermission(Permissions.FilesPermissionsManage)
            .WithTags(Tags.Files);
    }
}




