using Courier.Domain.Enums;
using Shared.Domain.Enums;

namespace Courier.Domain.DTOs.Responses;

public record TemplateDto(
   Guid Id,
   string Module,
   string Key,
   bool IsAllowingOptOut,
   NotificationSeverity Severity,
   RetentionPolicy RetentionPolicy,
   DateTime CreatedAt,
   Guid CreatedBy,
   DateTime? UpdatedAt,
   Guid? UpdatedBy,
   IReadOnlyCollection<TemplateTranslationDto> Translations);
