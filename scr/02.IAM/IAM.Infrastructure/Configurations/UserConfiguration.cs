using IAM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Infrastructure.Configurations;

namespace IAM.Infrastructure.Configurations;

public class UserConfiguration : BaseAuditedConfiguration<User>
{
   public override void Configure(EntityTypeBuilder<User> builder)
   {
      base.Configure(builder);

      builder.Property(u => u.Name).IsRequired().HasMaxLength(100);
      builder.Property(u => u.Email).IsRequired().HasMaxLength(256);
      builder.Property(u => u.PasswordHash).IsRequired().HasMaxLength(256);
      builder.Property(u => u.PasswordExpiresAt);
      builder.Property(u => u.IsActive).IsRequired().HasDefaultValue(true);
      builder.Property(u => u.IsSystemAdmin).IsRequired().HasDefaultValue(false);
      builder.Property(u => u.EmailVerifiedAt);
      builder.Property(u => u.LastLoginAt);
      builder.Property(u => u.OrganizationId).IsRequired();

      builder.HasIndex(u => new { u.OrganizationId, u.Email }).IsUnique();

      // Foreign key
      builder.HasOne(u => u.Organization)
             .WithMany(c => c.Users)
             .HasForeignKey(u => u.OrganizationId)
             .OnDelete(DeleteBehavior.NoAction);
   }
}