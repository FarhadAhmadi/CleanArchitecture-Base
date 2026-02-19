using Domain.Notifications;
using Infrastructure.Database;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Notifications;

internal sealed class NotificationMessageConfiguration : IEntityTypeConfiguration<NotificationMessage>
{
    public void Configure(EntityTypeBuilder<NotificationMessage> builder)
    {
        builder.ToTable("NotificationMessages", Schemas.Notifications);
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.RecipientEncrypted).HasMaxLength(2000).IsRequired();
        builder.Property(x => x.RecipientHash).HasMaxLength(128);
        builder.Property(x => x.Subject).HasMaxLength(500).IsRequired();
        builder.Property(x => x.Body).HasMaxLength(8000).IsRequired();
        builder.Property(x => x.Language).HasMaxLength(20).IsRequired();
        builder.Property(x => x.LastError).HasMaxLength(2000);
        builder.Property(x => x.Channel).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.Priority).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<NotificationTemplate>()
            .WithMany()
            .HasForeignKey(x => x.TemplateId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => new { x.Status, x.CreatedAtUtc });
        builder.HasIndex(x => new { x.Channel, x.Status });
        builder.HasIndex(x => x.CreatedByUserId);
    }
}
