using Domain.Modules.Scheduler;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Scheduler;

internal sealed class JobScheduleConfiguration : IEntityTypeConfiguration<JobSchedule>
{
    public void Configure(EntityTypeBuilder<JobSchedule> builder)
    {
        builder.ToTable("JobSchedules", Schemas.Scheduler);
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.Type).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.CronExpression).HasMaxLength(120);
        builder.Property(x => x.IsEnabled).IsRequired();
        builder.Property(x => x.MisfirePolicy).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.MaxCatchUpRuns).IsRequired();
        builder.Property(x => x.RetryAttempt).IsRequired();

        builder.HasOne<ScheduledJob>()
            .WithMany()
            .HasForeignKey(x => x.JobId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.JobId).IsUnique();
        builder.HasIndex(x => new { x.IsEnabled, x.NextRunAtUtc });
        builder.HasIndex(x => x.MisfirePolicy);
    }
}
