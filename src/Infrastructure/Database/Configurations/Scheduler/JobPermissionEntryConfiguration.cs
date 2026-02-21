using Domain.Modules.Scheduler;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Scheduler;

internal sealed class JobPermissionEntryConfiguration : IEntityTypeConfiguration<JobPermissionEntry>
{
    public void Configure(EntityTypeBuilder<JobPermissionEntry> builder)
    {
        builder.ToTable("JobPermissionEntries", Schemas.Scheduler);
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.SubjectType).HasMaxLength(20).IsRequired();
        builder.Property(x => x.SubjectValue).HasMaxLength(150).IsRequired();

        builder.HasOne<ScheduledJob>()
            .WithMany()
            .HasForeignKey(x => x.JobId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.JobId, x.SubjectType, x.SubjectValue }).IsUnique();
    }
}

