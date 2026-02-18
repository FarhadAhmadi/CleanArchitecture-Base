using Domain.Files;
using Domain.Users;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Files;

internal sealed class FileAssetConfiguration : IEntityTypeConfiguration<FileAsset>
{
    public void Configure(EntityTypeBuilder<FileAsset> builder)
    {
        builder.ToTable("FileAssets", Schemas.Default);
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.OwnerUserId).IsRequired();
        builder.Property(x => x.ObjectKey).HasMaxLength(400).IsRequired();
        builder.Property(x => x.FileName).HasMaxLength(260).IsRequired();
        builder.Property(x => x.Extension).HasMaxLength(20).IsRequired();
        builder.Property(x => x.ContentType).HasMaxLength(150).IsRequired();
        builder.Property(x => x.SizeBytes).IsRequired();
        builder.Property(x => x.Module).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Folder).HasMaxLength(300);
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property(x => x.Sha256).HasMaxLength(64).IsRequired();
        builder.Property(x => x.UploadedAtUtc).IsRequired();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.OwnerUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.ObjectKey).IsUnique();
        builder.HasIndex(x => new { x.Module, x.UploadedAtUtc });
        builder.HasIndex(x => x.FileName);
        builder.HasIndex(x => x.IsDeleted);
    }
}
