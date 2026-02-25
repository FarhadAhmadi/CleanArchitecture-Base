using Application.Abstractions.Messaging;
using Web.Api.Endpoints.Users;
using Web.Api.Extensions;

using Application.Modules.Files;

namespace Web.Api.Endpoints.Files;

internal sealed class GetFileMetadata : IEndpoint, IOrderedEndpoint
{
    public int Order => 2;
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("files/{fileId:guid}", async (
                Guid fileId,
                IQueryHandler<GetFileMetadataQuery, IResult> handler,
                CancellationToken cancellationToken) =>
            (await handler.Handle(new GetFileMetadataQuery(fileId), cancellationToken)).Match(static x => x, Web.Api.Infrastructure.CustomResults.Problem))
            .HasPermission(Permissions.FilesRead)
            .WithTags(Tags.Files);
    }
}






