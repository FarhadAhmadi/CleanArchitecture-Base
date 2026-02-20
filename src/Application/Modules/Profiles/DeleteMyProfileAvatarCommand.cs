using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Profiles;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;

namespace Application.Profiles;

public sealed record DeleteMyProfileAvatarCommand : ICommand<IResult>;
internal sealed class DeleteMyProfileAvatarCommandHandler(
    IUserContext userContext,
    IApplicationDbContext writeContext) : ResultWrappingCommandHandler<DeleteMyProfileAvatarCommand>
{
    protected override async Task<IResult> HandleCore(DeleteMyProfileAvatarCommand command, CancellationToken cancellationToken)
    {
        _ = command;
        return await DeleteAsync(userContext, writeContext, cancellationToken);
    }

    private static async Task<IResult> DeleteAsync(
        IUserContext userContext,
        IApplicationDbContext writeContext,
        CancellationToken cancellationToken)
    {
        UserProfile? profile = await ProfileEndpointCommon.GetCurrentProfileForUpdateAsync(
            userContext.UserId,
            writeContext,
            cancellationToken);

        if (profile is null)
        {
            return Results.NotFound();
        }

        profile.AvatarFileId = null;
        profile.AvatarUrl = null;
        profile.LastProfileUpdateAtUtc = DateTime.UtcNow;
        profile.ProfileCompletenessScore = ProfileEndpointCommon.ComputeCompleteness(profile);
        profile.Raise(new UserProfileChangedDomainEvent(profile.Id, profile.UserId, "Avatar", profile.ProfileCompletenessScore));

        await writeContext.SaveChangesAsync(cancellationToken);
        return Results.Ok(ProfileEndpointCommon.ToPrivateResponse(profile));
    }
}





