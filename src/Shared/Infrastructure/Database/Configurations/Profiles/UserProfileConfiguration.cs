using Domain.Profiles;
using Domain.Users;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Profiles;

internal sealed class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
{
    public void Configure(EntityTypeBuilder<UserProfile> builder)
    {
        builder.ToTable("UserProfiles", Schemas.Profiles);
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.DisplayName).HasMaxLength(160).IsRequired();
        builder.Property(x => x.Bio).HasMaxLength(1200);
        builder.Property(x => x.AvatarUrl).HasMaxLength(800);
        builder.Property(x => x.AvatarFileId);
        builder.Property(x => x.Gender).HasMaxLength(32);
        builder.Property(x => x.Location).HasMaxLength(200);
        builder.Property(x => x.TimeZone).HasMaxLength(80);
        builder.Property(x => x.PreferredLanguage).HasMaxLength(16).IsRequired();
        builder.Property(x => x.WebsiteUrl).HasMaxLength(400);
        builder.Property(x => x.ContactEmail).HasMaxLength(320);
        builder.Property(x => x.ContactPhone).HasMaxLength(32);
        builder.Property(x => x.FavoriteMusicTitle).HasMaxLength(200);
        builder.Property(x => x.FavoriteMusicArtist).HasMaxLength(200);
        builder.Property(x => x.FavoriteMusicFileId);
        builder.Property(x => x.InterestsCsv).HasMaxLength(3000).IsRequired();
        builder.Property(x => x.SocialLinksJson).HasMaxLength(4000).IsRequired();
        builder.Property(x => x.ProfileCompletenessScore).IsRequired();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.UserId).IsUnique();
        builder.HasIndex(x => x.IsProfilePublic);
        builder.HasIndex(x => x.DisplayName);
        builder.HasIndex(x => x.LastProfileUpdateAtUtc);
    }
}
