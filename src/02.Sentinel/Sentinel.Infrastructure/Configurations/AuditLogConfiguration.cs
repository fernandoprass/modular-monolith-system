using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sentinel.Domain;
using Sentinel.Domain.Entities;
using Shared.Infrastructure.Configurations;

namespace Sentinel.Infrastructure.Configurations;

public class AuditLogConfiguration : BaseConfiguration<AuditLog>
{
   public override void Configure(EntityTypeBuilder<AuditLog> builder)
   {
      base.Configure(builder);

      builder.Property(a => a.Timestamp).IsRequired();
      builder.Property(a => a.Module).IsRequired().HasMaxLength(50);
      builder.Property(a => a.Feature).IsRequired().HasMaxLength(100);
      builder.Property(a => a.Action).IsRequired().HasMaxLength(100);
      builder.Property(a => a.PrivacyLevel).IsRequired().HasConversion<byte>();
      builder.Property(a => a.Description).HasMaxLength(500);
      builder.Property(a => a.Entity).IsRequired().HasMaxLength(100);
      builder.Property(a => a.IpAddress).HasMaxLength(64);
      builder.Property(a => a.UserAgent).HasMaxLength(500);
      builder.Property(a => a.Metadata).HasColumnType(SentinelConst.Database.JsonbType);

      builder.HasIndex(a => new { a.OrganizationId, a.Timestamp, a.UserId });
      builder.HasIndex(a => new { a.OrganizationId, a.Timestamp, a.Module });
      builder.HasIndex(a => new { a.OrganizationId, a.Timestamp, a.EntityId });


   }
}
