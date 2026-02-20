using Application.Abstractions.Files;
using Application.Abstractions.Messaging;
using Application.Modules.Files;
using Web.Api.Endpoints.Users;
using Web.Api.Extensions;

namespace Web.Api.Endpoints.Files;

public sealed record ValidateFileRequest(string FileName, long SizeBytes, string? ContentType);

internal sealed class ValidateFile : IEndpoint, IOrderedEndpoint
{
    public int Order => 2;

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("files/validate", async (
                ValidateFileRequest request,
                ICommandHandler<ValidateFileCommand, IResult> handler,
                CancellationToken cancellationToken) =>
            (await handler.Handle(
                new ValidateFileCommand(new ValidateFileInput(request.FileName, request.SizeBytes, request.ContentType)),
                cancellationToken)).Match(static x => x, Web.Api.Infrastructure.CustomResults.Problem))
            .HasPermission(Permissions.FilesWrite)
            .WithTags(Tags.Files);
    }
}


