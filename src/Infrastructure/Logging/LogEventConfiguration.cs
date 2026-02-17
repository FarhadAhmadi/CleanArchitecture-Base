using Domain.Logging;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Logging;

internal sealed class LogEventConfiguration : IEntityTypeConfiguration<LogEvent>
{
    public void Configure(EntityTypeBuilder<LogEvent> builder)
    {
        builder.ToTable("LogEvents", Schemas.Default);
        builder.HasKey(x => x.Id);

        builder.Property(x => x.IdempotencyKey).HasMaxLength(120);
        builder.Property(x => x.Message).HasMaxLength(4000).IsRequired();
        builder.Property(x => x.SourceService).HasMaxLength(150).IsRequired();
        builder.Property(x => x.SourceModule).HasMaxLength(150).IsRequired();
        builder.Property(x => x.TraceId).HasMaxLength(150).IsRequired();
        builder.Property(x => x.RequestId).HasMaxLength(150);
        builder.Property(x => x.TenantId).HasMaxLength(100);
        builder.Property(x => x.ActorType).HasMaxLength(50).IsRequired();
        builder.Property(x => x.ActorId).HasMaxLength(100);
        builder.Property(x => x.Outcome).HasMaxLength(50).IsRequired();
        builder.Property(x => x.SessionId).HasMaxLength(150);
        builder.Property(x => x.Ip).HasMaxLength(64);
        builder.Property(x => x.UserAgent).HasMaxLength(1000);
        builder.Property(x => x.DeviceInfo).HasMaxLength(1000);
        builder.Property(x => x.HttpMethod).HasMaxLength(20);
        builder.Property(x => x.HttpPath).HasMaxLength(1000);
        builder.Property(x => x.ErrorCode).HasMaxLength(200);
        builder.Property(x => x.ErrorStackHash).HasMaxLength(256);
        builder.Property(x => x.TagsCsv).HasMaxLength(2000);
        builder.Property(x => x.Checksum).HasMaxLength(128).IsRequired();

        builder.HasIndex(x => x.TimestampUtc);
        builder.HasIndex(x => x.Level);
        builder.HasIndex(x => x.TraceId);
        builder.HasIndex(x => x.ActorId);
        builder.HasIndex(x => x.SourceService);
        builder.HasIndex(x => x.IdempotencyKey).IsUnique().HasFilter("[IdempotencyKey] IS NOT NULL");
    }
}
