using Application.Abstractions.Messaging;
using Application.Users.Management;
using Microsoft.AspNetCore.Mvc;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Users;

public sealed class GetUsersRequest
{
    [FromQuery(Name = "search")]
    public string? Search { get; set; }
}

internal sealed class GetUsers : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("users", async (
            IQueryHandler<GetUsersQuery, List<UserAdminResponse>> handler,
            CancellationToken cancellationToken,
            [AsParameters] GetUsersRequest request) =>
        {
            Result<List<UserAdminResponse>> result = await handler.Handle(
                new GetUsersQuery(request.Search),
                cancellationToken);
            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Users)
        .HasPermission(Permissions.UsersAccess);
    }
}
