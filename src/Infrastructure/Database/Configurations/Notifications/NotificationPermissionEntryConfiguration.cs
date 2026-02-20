using Domain.Modules.Notifications;
using Domain.Notifications;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Notifications;

internal sealed class NotificationPermissionEntryConfiguration : IEntityTypeConfiguration<NotificationPermissionEntry>
{
    public void Configure(EntityTypeBuilder<NotificationPermissionEntry> builder)
    {
        builder.ToTable("NotificationPermissionEntries", Schemas.Notifications);
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.SubjectType).HasMaxLength(20).IsRequired();
        builder.Property(x => x.SubjectValue).HasMaxLength(150).IsRequired();

        builder.HasOne<NotificationMessage>()
            .WithMany()
            .HasForeignKey(x => x.NotificationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.NotificationId, x.SubjectType, x.SubjectValue }).IsUnique();
    }
}
