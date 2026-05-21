namespace Courier.Domain.DTOs.Requests;

public record EmailTemplateTranslationRequest(
   string Language,
   string Name,
   string Subject,
   string Body);
