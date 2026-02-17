using Domain.Auditing;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Auditing;

internal sealed class AuditEntryConfiguration : IEntityTypeConfiguration<AuditEntry>
{
    public void Configure(EntityTypeBuilder<AuditEntry> builder)
    {
        builder.ToTable("AuditEntries", Schemas.Default);
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ActorId).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Action).HasMaxLength(150).IsRequired();
        builder.Property(x => x.ResourceType).HasMaxLength(100).IsRequired();
        builder.Property(x => x.ResourceId).HasMaxLength(200).IsRequired();
        builder.Property(x => x.PayloadHash).HasMaxLength(128).IsRequired();
        builder.Property(x => x.PreviousChecksum).HasMaxLength(128).IsRequired();
        builder.Property(x => x.Checksum).HasMaxLength(128).IsRequired();

        builder.HasIndex(x => x.TimestampUtc);
        builder.HasIndex(x => x.ActorId);
        builder.HasIndex(x => x.Action);
    }
}
