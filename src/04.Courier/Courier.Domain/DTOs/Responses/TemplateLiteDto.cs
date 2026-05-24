using Courier.Domain.Enums;

namespace Courier.Domain.DTOs.Responses;

public record TemplateLiteDto(
   Guid Id,
   string Key,
   string Name,
   TemplateType Type,
   RetentionPolicy RetentionPolicy
   );
