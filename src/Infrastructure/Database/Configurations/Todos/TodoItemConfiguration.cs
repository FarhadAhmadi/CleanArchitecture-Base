using Domain.Todos;
using Domain.Users;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Todos;

internal sealed class TodoItemConfiguration : IEntityTypeConfiguration<TodoItem>
{
    public void Configure(EntityTypeBuilder<TodoItem> builder)
    {
        builder.ToTable("TodoItems", Schemas.Todos);
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id).ValueGeneratedNever();
        builder.Property(t => t.UserId).IsRequired();
        builder.Property(t => t.Description).HasMaxLength(1000).IsRequired();
        builder.Property(t => t.CreatedAt).IsRequired();
        builder.Property(t => t.Priority).IsRequired();

        builder.Property(t => t.DueDate).HasConversion(
            d => d != null ? DateTime.SpecifyKind(d.Value, DateTimeKind.Utc) : d,
            v => v);

        builder.Property(t => t.CompletedAt).HasConversion(
            d => d != null ? DateTime.SpecifyKind(d.Value, DateTimeKind.Utc) : d,
            v => v);

        builder.HasOne<User>().WithMany().HasForeignKey(t => t.UserId).OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(t => new { t.UserId, t.IsCompleted });
        builder.HasIndex(t => t.CreatedAt);
    }
}
