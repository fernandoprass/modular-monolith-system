using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Domain;
using Shared.Domain.Entities;
using Shared.Domain.Enums;

namespace Shared.Infrastructure.Configurations;

public class ParameterConfiguration : BaseAuditedConfiguration<Parameter>
{
   public override void Configure(EntityTypeBuilder<Parameter> builder)
   {
      base.Configure(builder);

      builder.Property(p => p.Module).IsRequired().HasMaxLength(100);
      builder.Property(p => p.Group).IsRequired().HasMaxLength(100);
      builder.Property(p => p.Name).IsRequired().HasMaxLength(200);
      builder.Property(p => p.Key).IsRequired().HasMaxLength(402);
      builder.Property(p => p.Title).IsRequired().HasMaxLength(255);
      builder.Property(p => p.Description).IsRequired().HasColumnType(SharedConst.Database.TextType);
      builder.Property(p => p.Type).IsRequired();
      builder.Property(p => p.Value).IsRequired().HasColumnType(SharedConst.Database.TextType);
      builder.Property(p => p.ValidationRegex).IsRequired(false).HasMaxLength(255);
      builder.Property(p => p.ValidationErrorCustomMessage).IsRequired(false).HasMaxLength(255);
      builder.Property(p => p.ListItems).IsRequired(false).HasColumnType(SharedConst.Database.TextType);
      builder.Property(p => p.ExternalListEndpoint).IsRequired(false).HasMaxLength(500);
      builder.Property(p => p.OverrideType).IsRequired().HasDefaultValue(ParameterOverrideType.None);
      builder.Property(p => p.IsVisible).IsRequired().HasDefaultValue(true);

      builder.HasIndex(p => new {p.Module, p.Group, p.Name }).IsUnique();

      builder.HasIndex(p => new {p.Key }).IsUnique();
   }
}
