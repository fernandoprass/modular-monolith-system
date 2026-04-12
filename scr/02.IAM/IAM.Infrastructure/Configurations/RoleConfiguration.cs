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
      builder.Property(r => r.Description).IsRequired().HasColumnType(SharedConst.Database.TextType);
      builder.Property(r => r.IsDefault).IsRequired().HasDefaultValue(false);
      builder.Property(r => r.IsActive).IsRequired().HasDefaultValue(true);
      builder.Property(r => r.CustomerId).IsRequired(false);


      builder.HasIndex(r => new { r.Name, r.CustomerId }).IsUnique();

      builder.HasMany(r => r.RolePermissions)
         .WithOne(rf => rf.Role)
         .HasForeignKey(rf => rf.RoleId)
         .OnDelete(DeleteBehavior.Cascade);

      builder.HasMany(r => r.UserRoles)
         .WithOne(ur => ur.Role)
         .HasForeignKey(ur => ur.RoleId)
         .OnDelete(DeleteBehavior.Restrict);

      //todo fix it
      //builder.HasOne(r => r.Customer)
      // .WithMany(c => c.Users)
      // .HasForeignKey(u => u.CustomerId)
      // .OnDelete(DeleteBehavior.NoAction);
   }
}
