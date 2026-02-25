using Domain.Modules.Scheduler;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Scheduler;

internal sealed class JobExecutionConfiguration : IEntityTypeConfiguration<JobExecution>
{
    public void Configure(EntityTypeBuilder<JobExecution> builder)
    {
        builder.ToTable("JobExecutions", Schemas.Scheduler);
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.TriggeredBy).HasMaxLength(100).IsRequired();
        builder.Property(x => x.NodeId).HasMaxLength(150);
        builder.Property(x => x.DeadLetterReason).HasMaxLength(2000);
        builder.Property(x => x.PayloadSnapshotJson).HasMaxLength(8000);
        builder.Property(x => x.Error).HasMaxLength(2000);

        builder.HasOne<ScheduledJob>()
            .WithMany()
            .HasForeignKey(x => x.JobId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.JobId, x.StartedAtUtc });
        builder.HasIndex(x => new { x.JobId, x.ScheduledAtUtc });
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.IsDeadLetter);
    }
}
