using Domain.Logging;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Logging;

internal sealed class AlertIncidentConfiguration : IEntityTypeConfiguration<AlertIncident>
{
    public void Configure(EntityTypeBuilder<AlertIncident> builder)
    {
        builder.ToTable("AlertIncidents", Schemas.Default);
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.Status).HasMaxLength(50).IsRequired();
        builder.Property(x => x.LastError).HasMaxLength(2000);

        builder.HasOne<AlertRule>()
            .WithMany()
            .HasForeignKey(x => x.RuleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<LogEvent>()
            .WithMany()
            .HasForeignKey(x => x.TriggerEventId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.RuleId);
        builder.HasIndex(x => x.TriggeredAtUtc);
        builder.HasIndex(x => x.Status);
    }
}
