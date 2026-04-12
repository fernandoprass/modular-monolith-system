using IAM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAM.Infrastructure.Configurations;

public class RoleFeatureConfiguration : IEntityTypeConfiguration<RolePermission>
{
   public void Configure(EntityTypeBuilder<RolePermission> builder)
   {
      builder.HasKey(rf => new { rf.RoleId, rf.PermissionId });

      builder.HasOne(rf => rf.Role)
         .WithMany(r => r.RolePermissions)
         .HasForeignKey(rf => rf.RoleId);

      builder.HasOne(rf => rf.Permission)
         .WithMany(f => f.RoleFeatures)
         .HasForeignKey(rf => rf.PermissionId);
   }
}
