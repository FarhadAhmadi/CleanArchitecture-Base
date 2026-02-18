using Domain.Users;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Users;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users", Schemas.Default);
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).ValueGeneratedNever();
        builder.Property(u => u.Email).HasMaxLength(320).IsRequired();
        builder.Property(u => u.FirstName).HasMaxLength(120).IsRequired();
        builder.Property(u => u.LastName).HasMaxLength(120).IsRequired();
        builder.Property(u => u.PasswordHash).HasMaxLength(4000).IsRequired();

        builder.HasIndex(u => u.Email).IsUnique();
    }
}
