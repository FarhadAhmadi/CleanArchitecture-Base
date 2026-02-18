using Domain.Users;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Users;

internal sealed class UserExternalLoginConfiguration : IEntityTypeConfiguration<UserExternalLogin>
{
    public void Configure(EntityTypeBuilder<UserExternalLogin> builder)
    {
        builder.ToTable("UserExternalLogins", Schemas.Default);
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.Provider).HasMaxLength(40).IsRequired();
        builder.Property(x => x.ProviderUserId).HasMaxLength(300).IsRequired();
        builder.Property(x => x.Email).HasMaxLength(320);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.Provider, x.ProviderUserId }).IsUnique();
        builder.HasIndex(x => new { x.UserId, x.Provider }).IsUnique();
    }
}
