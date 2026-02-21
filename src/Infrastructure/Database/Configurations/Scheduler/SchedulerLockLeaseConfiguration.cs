using Domain.Modules.Scheduler;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Scheduler;

internal sealed class SchedulerLockLeaseConfiguration : IEntityTypeConfiguration<SchedulerLockLease>
{
    public void Configure(EntityTypeBuilder<SchedulerLockLease> builder)
    {
        builder.ToTable("SchedulerLocks", Schemas.Scheduler);
        builder.HasKey(x => x.LockName);

        builder.Property(x => x.LockName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.OwnerNodeId).HasMaxLength(150).IsRequired();

        builder.HasIndex(x => x.ExpiresAtUtc);
    }
}
