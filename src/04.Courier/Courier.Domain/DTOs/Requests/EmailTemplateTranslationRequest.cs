namespace Courier.Domain.DTOs.Requests;

public record EmailTemplateTranslationRequest(
   string Language,
   string Subject,
   string Body);
