using IAM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Domain;
using Shared.Infrastructure.Configurations;

namespace IAM.Infrastructure.Configurations;

public class RoleConfiguration : BaseAuditedConfiguration<Role>
{
   public override void Configure(EntityTypeBuilder<Role> builder)
   {
      base.Configure(builder);

      builder.Property(r => r.Name).IsRequired().HasMaxLength(100);
      builder.Property(r => r.Description).IsRequired().HasColumnType(SharedConst.Database.PostgreSQL.TextType);
      builder.Property(r => r.IsDefault).IsRequired().HasDefaultValue(false);
      builder.Property(r => r.IsActive).IsRequired().HasDefaultValue(true);
      builder.Property(r => r.OrganizationId).IsRequired(false);

      builder.HasIndex(r => r.Name);

      builder.HasMany(r => r.RolePermissions)
         .WithOne(rf => rf.Role)
         .HasForeignKey(rf => rf.RoleId)
         .OnDelete(DeleteBehavior.Cascade);

      builder.HasMany(r => r.UserRoles)
         .WithOne(ur => ur.Role)
         .HasForeignKey(ur => ur.RoleId)
         .OnDelete(DeleteBehavior.Restrict);

      builder.HasOne(r => r.Organization)
             .WithMany(r => r.Roles)
             .HasForeignKey(r => r.OrganizationId)
             .OnDelete(DeleteBehavior.NoAction);
   }
}
