using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Integration;

internal sealed class InboxMessageConfiguration : IEntityTypeConfiguration<InboxMessage>
{
    public void Configure(EntityTypeBuilder<InboxMessage> builder)
    {
        builder.ToTable("InboxMessages", Schemas.Default);
        builder.HasKey(x => x.Id);

        builder.Property(x => x.MessageId).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Type).HasMaxLength(500).IsRequired();
        builder.Property(x => x.Error).HasMaxLength(2000);

        builder.HasIndex(x => x.MessageId).IsUnique();
        builder.HasIndex(x => x.ReceivedOnUtc);
        builder.HasIndex(x => x.ProcessedOnUtc);
    }
}
