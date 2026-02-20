using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Domain.Profiles;
using Microsoft.EntityFrameworkCore;
using Web.Api.Extensions;

namespace Web.Api.Endpoints.Profiles;

internal sealed class GetMyProfile : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("profiles/me", GetAsync)
            .HasPermission(Permissions.ProfilesRead)
            .WithTags(Tags.Profiles);
    }

    private static async Task<IResult> GetAsync(
        IUserContext userContext,
        IApplicationReadDbContext readContext,
        CancellationToken cancellationToken)
    {
        UserProfile? profile = await readContext.UserProfiles
            .SingleOrDefaultAsync(x => x.UserId == userContext.UserId, cancellationToken);

        return profile is null
            ? Results.NotFound()
            : Results.Ok(ProfileEndpointCommon.ToPrivateResponse(profile));
    }
}
