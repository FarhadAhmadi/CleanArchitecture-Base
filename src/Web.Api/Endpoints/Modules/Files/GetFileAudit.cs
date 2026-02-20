using Application.Abstractions.Messaging;
using Web.Api.Endpoints.Users;
using Web.Api.Extensions;

using Application.Modules.Files;

namespace Web.Api.Endpoints.Files;

internal sealed class GetFileAudit : IEndpoint, IOrderedEndpoint
{
    public int Order => 6;
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("files/audit/{fileId:guid}", async (
                Guid fileId,
                IQueryHandler<GetFileAuditQuery, IResult> handler,
                CancellationToken cancellationToken) =>
            (await handler.Handle(new GetFileAuditQuery(fileId), cancellationToken)).Match(static x => x, Web.Api.Infrastructure.CustomResults.Problem))
            .HasPermission(Permissions.FilesRead)
            .WithTags(Tags.Files);
    }
}






