namespace Courier.Domain.DTOs.Responses;

public record EmailTemplateTranslationDto(
   string Language,
   string Name,
   string Subject,
   string Body);
