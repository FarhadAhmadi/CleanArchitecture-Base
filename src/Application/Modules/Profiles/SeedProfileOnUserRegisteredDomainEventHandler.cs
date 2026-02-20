using Application.Abstractions.Data;
using Domain.Profiles;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Profiles;

internal sealed class SeedProfileOnUserRegisteredDomainEventHandler(
    IApplicationDbContext context,
    IDateTimeProvider dateTimeProvider)
    : IDomainEventHandler<UserRegisteredDomainEvent>
{
    public async Task Handle(UserRegisteredDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        bool exists = await context.UserProfiles
            .AsNoTracking()
            .AnyAsync(x => x.UserId == domainEvent.UserId, cancellationToken);

        if (exists)
        {
            return;
        }

        User? user = await context.Users
            .SingleOrDefaultAsync(x => x.Id == domainEvent.UserId, cancellationToken);

        if (user is null)
        {
            return;
        }

        DateTime now = dateTimeProvider.UtcNow;

        UserProfile profile = new()
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            DisplayName = ResolveDisplayName(user),
            ContactEmail = user.Email,
            ContactPhone = user.PhoneNumber,
            PreferredLanguage = "fa-IR",
            IsProfilePublic = true,
            InterestsCsv = string.Empty,
            SocialLinksJson = "{}",
            LastProfileUpdateAtUtc = now
        };

        profile.ProfileCompletenessScore = ComputeCompleteness(profile);
        profile.Raise(new UserProfileChangedDomainEvent(profile.Id, profile.UserId, "Seeded", profile.ProfileCompletenessScore));

        context.UserProfiles.Add(profile);
    }

    private static string ResolveDisplayName(User user)
    {
        string fullName = $"{user.FirstName} {user.LastName}".Trim();
        if (!string.IsNullOrWhiteSpace(fullName))
        {
            return fullName.Length > 160 ? fullName[..160] : fullName;
        }

        if (!string.IsNullOrWhiteSpace(user.UserName))
        {
            return user.UserName.Length > 160 ? user.UserName[..160] : user.UserName;
        }

        if (!string.IsNullOrWhiteSpace(user.Email))
        {
            string alias = user.Email.Split('@', StringSplitOptions.RemoveEmptyEntries)[0];
            if (!string.IsNullOrWhiteSpace(alias))
            {
                return alias.Length > 160 ? alias[..160] : alias;
            }
        }

        return "User";
    }

    private static int ComputeCompleteness(UserProfile profile)
    {
        int score = 0;
        int total = 10;

        if (!string.IsNullOrWhiteSpace(profile.DisplayName))
        {
            score++;
        }

        if (!string.IsNullOrWhiteSpace(profile.ContactEmail))
        {
            score++;
        }

        if (!string.IsNullOrWhiteSpace(profile.ContactPhone))
        {
            score++;
        }

        return (int)Math.Round(score / (double)total * 100, MidpointRounding.AwayFromZero);
    }
}


