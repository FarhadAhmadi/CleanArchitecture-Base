using Domain.Notifications;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Notifications;

internal sealed class NotificationScheduleConfiguration : IEntityTypeConfiguration<NotificationSchedule>
{
    public void Configure(EntityTypeBuilder<NotificationSchedule> builder)
    {
        builder.ToTable("NotificationSchedules", Schemas.Notifications);
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.RuleName).HasMaxLength(200);

        builder.HasOne<NotificationMessage>()
            .WithMany()
            .HasForeignKey(x => x.NotificationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.RunAtUtc, x.IsCancelled });
    }
}
