namespace Courier.Domain.DTOs.Requests;

public record TemplateEmailTranslationRequest(
   string Language,
   string Subject,
   string Body);
