using Domain.Modules.Scheduler;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Scheduler;

internal sealed class JobDependencyConfiguration : IEntityTypeConfiguration<JobDependency>
{
    public void Configure(EntityTypeBuilder<JobDependency> builder)
    {
        builder.ToTable("JobDependencies", Schemas.Scheduler);
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.HasOne<ScheduledJob>()
            .WithMany()
            .HasForeignKey(x => x.JobId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<ScheduledJob>()
            .WithMany()
            .HasForeignKey(x => x.DependsOnJobId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.JobId, x.DependsOnJobId }).IsUnique();
    }
}

