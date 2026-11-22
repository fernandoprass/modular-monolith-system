using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Domain;
using Shared.Domain.Entities;

namespace Shared.Infrastructure.Configurations;

internal class LookupConfiguration : BaseAuditedConfiguration<Lookup>
{
   public override void Configure(EntityTypeBuilder<Lookup> builder)
   {
      base.Configure(builder);
      builder.Property(x => x.Id).ValueGeneratedNever();
      builder.ToTable("lookups", table => table.HasCheckConstraint("ck_lookups_parent", "parent_id IS NULL OR parent_id <> id"));
      builder.Property(x => x.Type).IsRequired().HasMaxLength(SharedConst.Lookup.TypeMaxLength);
      builder.Property(x => x.Code).IsRequired().HasMaxLength(SharedConst.Lookup.CodeMaxLength);
      builder.HasOne(x => x.Parent).WithMany(x => x.Children).HasForeignKey(x => x.ParentId).OnDelete(DeleteBehavior.Cascade);
      builder.HasMany(x => x.Translations).WithOne(x => x.Lookup).HasForeignKey(x => x.LookupId).OnDelete(DeleteBehavior.Cascade);
      builder.Navigation(x => x.Children).UsePropertyAccessMode(PropertyAccessMode.Field);
      builder.Navigation(x => x.Translations).UsePropertyAccessMode(PropertyAccessMode.Field);
      builder.HasIndex(x => new { x.Type, x.OrganizationId }).IsUnique();
   }
}
