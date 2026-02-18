using Domain.Notifications;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Notifications;

internal sealed class NotificationDeliveryAttemptConfiguration : IEntityTypeConfiguration<NotificationDeliveryAttempt>
{
    public void Configure(EntityTypeBuilder<NotificationDeliveryAttempt> builder)
    {
        builder.ToTable("NotificationDeliveryAttempts", Schemas.Default);
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.Channel).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.ProviderMessageId).HasMaxLength(200);
        builder.Property(x => x.Error).HasMaxLength(2000);

        builder.HasOne<NotificationMessage>()
            .WithMany()
            .HasForeignKey(x => x.NotificationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.NotificationId, x.AttemptNumber });
        builder.HasIndex(x => new { x.IsSuccess, x.CreatedAtUtc });
    }
}
