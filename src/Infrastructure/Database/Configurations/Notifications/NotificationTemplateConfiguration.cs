using Domain.Notifications;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Notifications;

internal sealed class NotificationTemplateConfiguration : IEntityTypeConfiguration<NotificationTemplate>
{
    public void Configure(EntityTypeBuilder<NotificationTemplate> builder)
    {
        builder.ToTable("NotificationTemplates", Schemas.Notifications);
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Language).HasMaxLength(20).IsRequired();
        builder.Property(x => x.SubjectTemplate).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.BodyTemplate).HasMaxLength(10000).IsRequired();
        builder.Property(x => x.Channel).HasConversion<string>().HasMaxLength(20).IsRequired();

        builder.HasIndex(x => new { x.Name, x.Language, x.Channel }).IsUnique();
        builder.HasIndex(x => x.IsDeleted);
    }
}
