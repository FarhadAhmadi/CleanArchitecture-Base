using Application.Abstractions.Messaging;
using Application.Authorization.GetAccessControl;
using SharedKernel;
using Web.Api.Endpoints.Users;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Authorization;

internal sealed class GetAccessControl : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("authorization", async (
            IQueryHandler<GetAccessControlQuery, AccessControlResponse> handler,
            CancellationToken cancellationToken) =>
        {
            Result<AccessControlResponse> result = await handler.Handle(new GetAccessControlQuery(), cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Users)
        .HasPermission(Permissions.AuthorizationManage);
    }
}
