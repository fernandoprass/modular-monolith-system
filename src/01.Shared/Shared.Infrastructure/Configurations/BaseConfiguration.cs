using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Domain.Entities;

namespace Shared.Infrastructure.Configurations
{
   public abstract class BaseConfiguration<T> : IEntityTypeConfiguration<T> where T : Entity
   {
      public virtual void Configure(EntityTypeBuilder<T> builder)
      {
         builder.HasKey(x => x.Id);
      }
   }
}
