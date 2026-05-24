namespace Courier.Domain.DTOs.Responses;

public record TemplateEmailTranslationDto(
   bool IsHtml,
   string Language,
   string Subject,
   string Body);
