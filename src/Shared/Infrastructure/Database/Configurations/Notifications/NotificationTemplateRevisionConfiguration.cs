using Domain.Notifications;
using Infrastructure.Database;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Notifications;

internal sealed class NotificationTemplateRevisionConfiguration : IEntityTypeConfiguration<NotificationTemplateRevision>
{
    public void Configure(EntityTypeBuilder<NotificationTemplateRevision> builder)
    {
        builder.ToTable("NotificationTemplateRevisions", Schemas.Notifications);
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.SubjectTemplate).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.BodyTemplate).HasMaxLength(10000).IsRequired();

        builder.HasOne<NotificationTemplate>()
            .WithMany()
            .HasForeignKey(x => x.TemplateId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.ChangedByUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => new { x.TemplateId, x.Version }).IsUnique();
    }
}
