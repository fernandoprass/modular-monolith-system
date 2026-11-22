using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Domain;
using Shared.Domain.Entities;

namespace Shared.Infrastructure.Configurations;

internal class LookupTranslationConfiguration : BaseAuditedConfiguration<LookupTranslation>
{
   public override void Configure(EntityTypeBuilder<LookupTranslation> builder)
   {
      base.Configure(builder);
      builder.Property(x => x.Id).ValueGeneratedNever();
      builder.ToTable("lookup_translations");
      builder.Property(x => x.LanguageCode).IsRequired().HasMaxLength(SharedConst.Lookup.LanguageMaxLength);
      builder.Property(x => x.Code).IsRequired().HasMaxLength(SharedConst.Lookup.CodeMaxLength);
      builder.Property(x => x.Value).IsRequired().HasMaxLength(SharedConst.Lookup.ValueMaxLength);
      builder.HasIndex(x => new { x.LookupId, x.LanguageCode, x.Code }).IsUnique();
   }
}
