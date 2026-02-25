using Infrastructure.Database;
using Infrastructure.Integration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Integration;

internal sealed class IdempotencyRequestConfiguration : IEntityTypeConfiguration<IdempotencyRequest>
{
    public void Configure(EntityTypeBuilder<IdempotencyRequest> builder)
    {
        builder.ToTable("IdempotencyRequests", Schemas.Integration);
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Key).HasMaxLength(120).IsRequired();
        builder.Property(x => x.ScopeHash).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Scope).HasMaxLength(200).IsRequired();
        builder.Property(x => x.ContentType).HasMaxLength(120);

        builder.HasIndex(x => new { x.ScopeHash, x.Key }).IsUnique();
        builder.HasIndex(x => x.ExpiresAtUtc);
        builder.HasIndex(x => new { x.IsCompleted, x.CreatedAtUtc });
    }
}
