using Application.Abstractions.Messaging;
using Application.Authorization.Roles;
using SharedKernel;
using Web.Api.Endpoints.Users;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Authorization;

internal sealed class GetRoleById : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("authorization/roles/{roleId:guid}", async (
            Guid roleId,
            IQueryHandler<GetRoleByIdQuery, RoleCrudResponse> handler,
            CancellationToken cancellationToken) =>
        {
            Result<RoleCrudResponse> result = await handler.Handle(new GetRoleByIdQuery(roleId), cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Users)
        .HasPermission(Permissions.AuthorizationManage);
    }
}
