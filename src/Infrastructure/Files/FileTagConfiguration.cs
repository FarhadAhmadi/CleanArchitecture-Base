using Domain.Files;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Files;

internal sealed class FileTagConfiguration : IEntityTypeConfiguration<FileTag>
{
    public void Configure(EntityTypeBuilder<FileTag> builder)
    {
        builder.ToTable("FileTags", Schemas.Default);
        builder.HasKey(x => new { x.FileId, x.Tag });

        builder.Property(x => x.Tag).HasMaxLength(80).IsRequired();

        builder.HasOne<FileAsset>()
            .WithMany()
            .HasForeignKey(x => x.FileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.Tag);
    }
}
