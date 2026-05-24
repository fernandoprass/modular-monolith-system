using Courier.Domain.Enums;

namespace Courier.Domain.DTOs.Responses;

public record TemplateDto(
   Guid Id,
   string Key,
   string Name,
   TemplateType Type,
   RetentionPolicy RetentionPolicy,
   DateTime CreatedAt,
   Guid CreatedBy,
   DateTime? UpdatedAt,
   Guid? UpdatedBy,
   IReadOnlyCollection<TemplateEmailTranslationDto> EmailTranslations);
