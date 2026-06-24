namespace Courier.Domain.DTOs.Requests;

public record TemplateTranslationRequest(
   string Language,
   string Name,
   TemplateTranslationEmailRequest? Email,
   TemplateTranslationNotificationRequest? Notification);

public record TemplateTranslationEmailRequest(
   string Subject,
   string Body);

public record TemplateTranslationNotificationRequest(
   string Title,
   string Message,
   string? ActionLink);
