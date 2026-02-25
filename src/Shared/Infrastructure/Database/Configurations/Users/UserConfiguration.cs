using Domain.Users;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Users;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users", Schemas.Users);
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).ValueGeneratedNever();

        builder.Property(u => u.UserName).HasMaxLength(320);
        builder.Property(u => u.NormalizedUserName).HasMaxLength(320);
        builder.Property(u => u.Email).HasMaxLength(320);
        builder.Property(u => u.NormalizedEmail).HasMaxLength(320);
        builder.Property(u => u.PasswordHash).HasMaxLength(4000);
        builder.Property(u => u.SecurityStamp).HasMaxLength(128);
        builder.Property(u => u.ConcurrencyStamp).HasMaxLength(128);
        builder.Property(u => u.PhoneNumber).HasMaxLength(32);

        builder.Property(u => u.FirstName).HasMaxLength(120).IsRequired();
        builder.Property(u => u.LastName).HasMaxLength(120).IsRequired();

        builder.HasIndex(u => u.NormalizedEmail).IsUnique();
        builder.HasIndex(u => u.NormalizedUserName).IsUnique();
    }
}
