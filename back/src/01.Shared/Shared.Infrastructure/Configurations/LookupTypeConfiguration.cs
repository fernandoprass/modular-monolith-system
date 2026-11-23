using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Domain;
using Shared.Domain.Entities;
using Shared.Domain.Enums;

namespace Shared.Infrastructure.Configurations;

internal class LookupTypeConfiguration : IEntityTypeConfiguration<LookupType>
{
   public void Configure(EntityTypeBuilder<LookupType> builder)
   {
      builder.HasKey(x => x.Type);
      builder.Property(x => x.Type).IsRequired().HasMaxLength(SharedConst.Lookup.TypeMaxLength).ValueGeneratedNever();
      builder.ToTable("lookup_types", table =>
      {
         table.HasCheckConstraint("ck_lookup_types_type", "type <> '' AND type = lower(btrim(type))");
         table.HasCheckConstraint("ck_lookup_types_code_max_length",
            $"code_max_length > 0 AND code_max_length <= {SharedConst.Lookup.CodeMaxLength}");
         table.HasCheckConstraint("ck_lookup_types_code_padding_width",
            "code_padding_width IS NULL OR (code_padding_width > 0 AND code_padding_width <= code_max_length)");
         table.HasCheckConstraint("ck_lookup_types_code_format",
            $"code_format IN ({(byte)LookupCodeFormat.Text}, {(byte)LookupCodeFormat.Numeric})");
         table.HasCheckConstraint("ck_lookup_types_code_generation_mode",
            $"code_generation_mode IN ({(byte)LookupCodeGenerationMode.Manual}, {(byte)LookupCodeGenerationMode.Suggested}, {(byte)LookupCodeGenerationMode.Automatic})");
      });
   }
}
