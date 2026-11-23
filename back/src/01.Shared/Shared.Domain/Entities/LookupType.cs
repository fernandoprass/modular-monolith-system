using Shared.Domain.Enums;

namespace Shared.Domain.Entities;

internal class LookupType
{
   public string Type { get; private set; } = string.Empty;
   public bool AllowTenantEdit { get; private set; }
   public LookupCodeFormat CodeFormat { get; private set; }
   public int CodeMaxLength { get; private set; }
   public int? CodePaddingWidth { get; private set; }
   public LookupCodeGenerationMode CodeGenerationMode { get; private set; }

   private LookupType() { }

   public static LookupType Create(
      string type,
      bool allowTenantEdit,
      LookupCodeFormat codeFormat,
      int codeMaxLength,
      int? codePaddingWidth,
      LookupCodeGenerationMode codeGenerationMode)
   {
      return new LookupType
      {
         Type = type.Trim().ToLowerInvariant(),
         AllowTenantEdit = allowTenantEdit,
         CodeFormat = codeFormat,
         CodeMaxLength = codeMaxLength,
         CodePaddingWidth = codePaddingWidth,
         CodeGenerationMode = codeGenerationMode
      };
   }
}
