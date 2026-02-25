using Application.Abstractions.Messaging;
using Web.Api.Endpoints.Users;
using Web.Api.Extensions;

using Application.Modules.Files;

namespace Web.Api.Endpoints.Files;

internal sealed class SearchFilesByTag : IEndpoint, IOrderedEndpoint
{
    public int Order => 13;

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("files/tags/{tag}", async (
                string tag,
                IQueryHandler<SearchFilesByTagQuery, IResult> handler,
                CancellationToken cancellationToken) =>
            (await handler.Handle(new SearchFilesByTagQuery(tag), cancellationToken)).Match(static x => x, Web.Api.Infrastructure.CustomResults.Problem))
            .HasPermission(Permissions.FilesRead)
            .WithTags(Tags.Files);
    }
}




