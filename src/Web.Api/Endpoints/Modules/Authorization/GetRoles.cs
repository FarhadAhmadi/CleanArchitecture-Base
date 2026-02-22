using Application.Abstractions.Messaging;
using Application.Authorization.Roles;
using Microsoft.AspNetCore.Mvc;
using SharedKernel;
using Web.Api.Endpoints.Users;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Authorization;

public sealed class GetRolesRequest
{
    [FromQuery(Name = "search")]
    public string? Search { get; set; }
}

internal sealed class GetRoles : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("authorization/roles", async (
            IQueryHandler<GetRolesQuery, List<RoleCrudResponse>> handler,
            CancellationToken cancellationToken,
            [AsParameters] GetRolesRequest request) =>
        {
            Result<List<RoleCrudResponse>> result = await handler.Handle(
                new GetRolesQuery(request.Search),
                cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Users)
        .HasPermission(Permissions.AuthorizationManage);
    }
}
