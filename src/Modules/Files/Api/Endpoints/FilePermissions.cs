using Application.Abstractions.Files;
using Application.Abstractions.Messaging;
using Application.Modules.Files;
using Web.Api.Endpoints.Users;
using Web.Api.Extensions;

namespace Web.Api.Endpoints.Files;

public sealed record UpsertFilePermissionRequest(string SubjectType, string SubjectValue, bool CanRead, bool CanWrite, bool CanDelete);

internal sealed class FilePermissions : IEndpoint, IOrderedEndpoint
{
    public int Order => 13;

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("files/permissions/{fileId:guid}", async (
                Guid fileId,
                UpsertFilePermissionRequest request,
                ICommandHandler<UpsertFilePermissionCommand, IResult> handler,
                CancellationToken cancellationToken) =>
            (await handler.Handle(
                new UpsertFilePermissionCommand(
                    fileId,
                    new UpsertFilePermissionInput(request.SubjectType, request.SubjectValue, request.CanRead, request.CanWrite, request.CanDelete)),
                cancellationToken)).Match(static x => x, Web.Api.Infrastructure.CustomResults.Problem))
            .HasPermission(Permissions.FilesPermissionsManage)
            .WithTags(Tags.Files);
    }
}


