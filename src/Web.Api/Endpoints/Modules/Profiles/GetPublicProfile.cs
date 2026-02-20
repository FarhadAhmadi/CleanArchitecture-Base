using Application.Abstractions.Data;
using Domain.Profiles;
using Microsoft.EntityFrameworkCore;
using Web.Api.Extensions;

namespace Web.Api.Endpoints.Profiles;

internal sealed class GetPublicProfile : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("profiles/{userId:guid}/public", GetAsync)
            .HasPermission(Permissions.ProfilesPublicRead)
            .WithTags(Tags.Profiles);
    }

    private static async Task<IResult> GetAsync(
        Guid userId,
        IApplicationReadDbContext readContext,
        CancellationToken cancellationToken)
    {
        UserProfile? profile = await readContext.UserProfiles
            .SingleOrDefaultAsync(x => x.UserId == userId, cancellationToken);

        if (profile is null || !profile.IsProfilePublic)
        {
            return Results.NotFound();
        }

        return Results.Ok(ProfileEndpointCommon.ToPublicResponse(profile));
    }
}
