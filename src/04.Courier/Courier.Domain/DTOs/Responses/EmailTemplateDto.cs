using Courier.Domain.Enums;

namespace Courier.Domain.DTOs.Responses;

public record EmailTemplateDto(
   Guid Id,
   string Key,
   EmailRetentionPolicy RetentionPolicy,
   DateTime CreatedAt,
   Guid CreatedBy,
   DateTime? UpdatedAt,
   Guid? UpdatedBy,
   IReadOnlyCollection<EmailTemplateTranslationDto> Translations);
