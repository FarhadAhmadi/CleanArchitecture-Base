using Domain.Modules.Scheduler;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Scheduler;

internal sealed class ScheduledJobConfiguration : IEntityTypeConfiguration<ScheduledJob>
{
    public void Configure(EntityTypeBuilder<ScheduledJob> builder)
    {
        builder.ToTable("ScheduledJobs", Schemas.Scheduler);
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(2000);
        builder.Property(x => x.Type).HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(x => x.PayloadJson).HasMaxLength(8000);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.LastExecutionStatus).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.MaxRetryAttempts).IsRequired();
        builder.Property(x => x.RetryBackoffSeconds).IsRequired();
        builder.Property(x => x.MaxExecutionSeconds).IsRequired();
        builder.Property(x => x.MaxConsecutiveFailures).IsRequired();
        builder.Property(x => x.ConsecutiveFailures).IsRequired();
        builder.Property(x => x.IsQuarantined).IsRequired();
        builder.Property(x => x.DeadLetterReason).HasMaxLength(2000);

        builder.HasIndex(x => x.Name).IsUnique();
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.IsQuarantined);
    }
}
