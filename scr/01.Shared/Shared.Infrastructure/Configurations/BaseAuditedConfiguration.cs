using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Domain.Entities;

namespace Shared.Infrastructure.Configurations
{
   public abstract class BaseAuditedConfiguration<T> : IEntityTypeConfiguration<T> where T : EntityAudited
   {
      public virtual void Configure(EntityTypeBuilder<T> builder)
      {
         builder.HasKey(x => x.Id);

         builder.Property(x => x.CreatedAt).IsRequired();
         builder.Property(x => x.CreatedBy).IsRequired();
         builder.Property(x => x.UpdatedAt).IsRequired(false);
         builder.Property(x => x.UpdatedBy).IsRequired(false);
      }
   }
}
