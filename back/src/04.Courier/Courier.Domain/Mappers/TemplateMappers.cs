using Courier.Domain.DTOs.Responses;
using Courier.Domain.Entities;
using Courier.Domain.ValueObjects;
using Shared.Domain;

namespace Courier.Domain.Mappers;

public static class TemplateMappers
{
   public static TemplateLiteDto ToTemplateLiteDto(this Template template, string language)
   {
      var normalizedLanguage = LanguageOptions.Normalize(language);
      var name = template.Translations
         .SingleOrDefault(translation => translation.Language == normalizedLanguage)
         ?.Name ?? string.Empty;

      return new TemplateLiteDto(
         template.Id,
         template.Module,
         template.Key,
         name,
         template.IsAllowingOptOut,
         template.Severity,
         template.RetentionPolicy);
   }

   public static TemplateDto ToTemplateDto(this Template template)
   {
      return new TemplateDto(
         template.Id,
         template.Module,
         template.Key,
         template.IsAllowingOptOut,
         template.Severity,
         template.RetentionPolicy,
         template.CreatedAt,
         template.CreatedBy,
         template.UpdatedAt,
         template.UpdatedBy,
         template.Translations.Select(t => t.ToTemplateTranslationDto()).ToArray());
   }

   private static TemplateTranslationDto ToTemplateTranslationDto(this TemplateTranslation translation)
   {
      return new TemplateTranslationDto(
         translation.Language,
         translation.Name,
         translation.Email?.ToTemplateTranslationEmailDto(),
         translation.Notification?.ToTemplateTranslationNotificationDto());
   }

   private static TemplateTranslationEmailDto ToTemplateTranslationEmailDto(this TemplateTranslationEmail email)
   {
      return new TemplateTranslationEmailDto(email.Subject, email.Body, email.IsHtml);
   }

   private static TemplateTranslationNotificationDto ToTemplateTranslationNotificationDto(
      this TemplateTranslationNotification notification)
   {
      return new TemplateTranslationNotificationDto(
         notification.Title,
         notification.Message,
         notification.ActionLink);
   }
}
