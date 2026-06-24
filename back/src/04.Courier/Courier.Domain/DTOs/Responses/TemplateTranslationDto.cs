namespace Courier.Domain.DTOs.Responses;

public record TemplateTranslationDto(
   string Language,
   string Name,
   TemplateTranslationEmailDto? Email,
   TemplateTranslationNotificationDto? Notification);

public record TemplateTranslationEmailDto(
   string Subject,
   string Body,
   bool IsHtml);

public record TemplateTranslationNotificationDto(
   string Title,
   string Message,
   string? ActionLink);
