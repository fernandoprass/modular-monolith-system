using Shared.Domain.Enums;

namespace Shared.Domain.DTOs.Responses;

public record LookupTypeDto(
   string Type,
   bool AllowTenantEdit,
   LookupCodeFormat CodeFormat,
   int CodeMaxLength,
   int? CodePaddingWidth,
   LookupCodeGenerationMode CodeGenerationMode);
