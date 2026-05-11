using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sentinel.Domain;
using Sentinel.Domain.Entities;
using Shared.Infrastructure.Configurations;

namespace Sentinel.Infrastructure.Configurations;

public class SystemLogConfiguration : BaseConfiguration<SystemLog>
{
   public override void Configure(EntityTypeBuilder<SystemLog> builder)
   {
      base.Configure(builder);

      builder.Property(s => s.Timestamp).IsRequired();
      builder.Property(s => s.Level).IsRequired().HasConversion<byte>();
      builder.Property(s => s.Status).IsRequired().HasConversion<byte>();
      builder.Property(s => s.Source).IsRequired().HasMaxLength(100);
      builder.Property(s => s.Message).IsRequired().HasMaxLength(2000);
      builder.Property(s => s.Exception).HasColumnType(SentinelConst.Database.TextType);
      builder.Property(s => s.StackTrace).HasColumnType(SentinelConst.Database.TextType);
      builder.Property(s => s.RequestId).HasMaxLength(100);
      builder.Property(s => s.PropertiesJson).HasColumnType(SentinelConst.Database.JsonbType);

      builder.HasIndex(s => new { s.OrganizationId, s.Timestamp, s.UserId });
      builder.HasIndex(s => new { s.OrganizationId, s.Timestamp, s.Source });
      builder.HasIndex(s => new { s.OrganizationId, s.Timestamp, s.RequestId });
   }
}
