using Domain.Files;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Files;

internal sealed class FileAccessAuditConfiguration : IEntityTypeConfiguration<FileAccessAudit>
{
    public void Configure(EntityTypeBuilder<FileAccessAudit> builder)
    {
        builder.ToTable("FileAccessAudits", Schemas.Default);
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.Action).HasMaxLength(80).IsRequired();
        builder.Property(x => x.IpAddress).HasMaxLength(64);
        builder.Property(x => x.UserAgent).HasMaxLength(1024);
        builder.Property(x => x.TimestampUtc).IsRequired();

        builder.HasOne<FileAsset>()
            .WithMany()
            .HasForeignKey(x => x.FileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.FileId, x.TimestampUtc });
        builder.HasIndex(x => x.UserId);
    }
}
