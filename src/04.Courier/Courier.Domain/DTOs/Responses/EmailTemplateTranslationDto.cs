namespace Courier.Domain.DTOs.Responses;

public record EmailTemplateTranslationDto(
   bool IsHtml,
   string Language,
   string Subject,
   string Body);
