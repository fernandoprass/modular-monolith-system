using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Domain;
using Shared.Domain.Entities;

namespace Shared.Infrastructure.Configurations;

public class ParameterOverrideConfiguration : BaseAuditedConfiguration<ParameterOverride>
{
   public override void Configure(EntityTypeBuilder<ParameterOverride> builder)
   {
      base.Configure(builder);

      builder.Property(p => p.Value).IsRequired().HasColumnType(SharedConst.Database.TextType);
      builder.Property(pc => pc.OwnerId).IsRequired();
      builder.Property(pc => pc.ParameterId).IsRequired();

      builder.HasOne<Parameter>()
             .WithMany(p => p.ParameterOwners)
             .HasForeignKey(pc => pc.ParameterId)
             .OnDelete(DeleteBehavior.Cascade);
   }
}
