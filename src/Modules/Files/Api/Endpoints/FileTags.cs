using Application.Abstractions.Files;
using Application.Abstractions.Messaging;
using Application.Modules.Files;
using Web.Api.Endpoints.Users;
using Web.Api.Extensions;

namespace Web.Api.Endpoints.Files;

public sealed record AddFileTagRequest(string Tag);

internal sealed class FileTags : IEndpoint, IOrderedEndpoint
{
    public int Order => 12;

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("files/{fileId:guid}/tags", async (
                Guid fileId,
                AddFileTagRequest request,
                ICommandHandler<AddFileTagCommand, IResult> handler,
                CancellationToken cancellationToken) =>
            (await handler.Handle(
                new AddFileTagCommand(fileId, new AddFileTagInput(request.Tag)),
                cancellationToken)).Match(static x => x, Web.Api.Infrastructure.CustomResults.Problem))
            .HasPermission(Permissions.FilesWrite)
            .WithTags(Tags.Files);
    }
}


