using IAM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Infrastructure.Configurations;

namespace IAM.Infrastructure.Configurations;

public class CustomerConfiguration : BaseAuditedConfiguration<Customer>
{
   public override void Configure(EntityTypeBuilder<Customer> builder)
   {
      base.Configure(builder);

      builder.Property(c => c.Type).IsRequired();
      builder.Property(c => c.Code).IsRequired().HasMaxLength(25);
      builder.Property(c => c.Name).IsRequired().HasMaxLength(100);
      builder.Property(c => c.Description).HasMaxLength(1000);
      builder.Property(c => c.IsActive).IsRequired().HasDefaultValue(true);
      builder.Property(c => c.IsMaster).IsRequired().HasDefaultValue(false);
   }
}