using IAM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Domain;
using Shared.Infrastructure.Configurations;

namespace IAM.Infrastructure.Configurations;

public class PermissionConfiguration : BaseAuditedConfiguration<Permission>
{
   public override void Configure(EntityTypeBuilder<Permission> builder)
   {
      base.Configure(builder);

      builder.Property(p => p.Module).IsRequired().HasMaxLength(50);
      builder.Property(p => p.Group).IsRequired().HasMaxLength(50);
      builder.Property(p => p.Name).IsRequired().HasMaxLength(100);
      builder.Property(p => p.Title).IsRequired().HasMaxLength(200);
      builder.Property(p => p.Description).HasColumnType(SharedConst.Database.TextType);
      builder.Property(p => p.IsActive).IsRequired().HasDefaultValue(true);

      builder.HasIndex(p => p.Name).IsUnique();

      builder.HasMany(p => p.RoleFeatures)
         .WithOne(rp => rp.Permission)
         .HasForeignKey(rp => rp.PermissionId)
         .OnDelete(DeleteBehavior.Cascade);
   }
}
