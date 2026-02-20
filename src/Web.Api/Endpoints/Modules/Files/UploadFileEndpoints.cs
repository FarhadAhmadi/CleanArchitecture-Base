using Microsoft.AspNetCore.Mvc;
using Application.Abstractions.Files;
using Application.Abstractions.Messaging;
using Application.Modules.Files;
using Web.Api.Endpoints.Users;
using Web.Api.Extensions;

namespace Web.Api.Endpoints.Files;

public sealed class UploadFileRequest
{
    [FromForm(Name = "file")]
    public IFormFile File { get; set; } = default!;

    [FromForm(Name = "module")]
    public string Module { get; set; } = "General";

    [FromForm(Name = "folder")]
    public string? Folder { get; set; }

    [FromForm(Name = "description")]
    public string? Description { get; set; }
}

internal sealed class UploadFileEndpoints : IEndpoint, IOrderedEndpoint
{
    public int Order => 1;

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("files", async (
                [FromForm] UploadFileRequest request,
                HttpContext httpContext,
                ICommandHandler<UploadFileCommand, IResult> handler,
                CancellationToken cancellationToken) =>
            (await handler.Handle(
                new UploadFileCommand(
                    new UploadFileInput
                    {
                        File = request.File,
                        Module = request.Module,
                        Folder = request.Folder,
                        Description = request.Description
                    },
                    httpContext),
                cancellationToken)).Match(static x => x, Web.Api.Infrastructure.CustomResults.Problem))
            .HasPermission(Permissions.FilesWrite)
            .RequireRateLimiting(Web.Api.DependencyInjection.GetRateLimiterPolicyName())
            .WithTags(Tags.Files);
    }
}

