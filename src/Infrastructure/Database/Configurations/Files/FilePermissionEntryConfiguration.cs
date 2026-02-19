using Domain.Files;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Files;

internal sealed class FilePermissionEntryConfiguration : IEntityTypeConfiguration<FilePermissionEntry>
{
    public void Configure(EntityTypeBuilder<FilePermissionEntry> builder)
    {
        builder.ToTable("FilePermissionEntries", Schemas.Files);
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.SubjectType).HasMaxLength(20).IsRequired();
        builder.Property(x => x.SubjectValue).HasMaxLength(150).IsRequired();
        builder.Property(x => x.CreatedAtUtc).IsRequired();

        builder.HasOne<FileAsset>()
            .WithMany()
            .HasForeignKey(x => x.FileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.FileId, x.SubjectType, x.SubjectValue }).IsUnique();
    }
}
