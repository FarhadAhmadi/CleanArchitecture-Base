using Application.Abstractions.Messaging;
using Web.Api.Extensions;

using Application.Modules.Files;

namespace Web.Api.Endpoints.Files;

internal sealed class GetPublicFileByLink : IEndpoint, IOrderedEndpoint
{
    public int Order => 7;

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("files/public/{token}", async (
                string token,
                HttpContext httpContext,
                IQueryHandler<GetPublicFileByLinkQuery, IResult> handler,
                CancellationToken cancellationToken) =>
            (await handler.Handle(new GetPublicFileByLinkQuery(token, httpContext), cancellationToken)).Match(static x => x, Web.Api.Infrastructure.CustomResults.Problem))
            .RequireRateLimiting(Web.Api.DependencyInjection.GetPublicFileLinkRateLimiterPolicyName())
            .WithTags(Tags.Files);
    }
}




