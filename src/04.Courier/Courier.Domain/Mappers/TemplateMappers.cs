using Courier.Domain.DTOs.Responses;
using Courier.Domain.Entities;
using Courier.Domain.ValueObjects;

namespace Courier.Domain.Mappers;

public static class TemplateMappers
{
   public static TemplateLiteDto ToTemplateLiteDto(this Template template)
   {
      return new TemplateLiteDto(
         template.Id,
         template.Key,
         template.Name,
         template.Type,
         template.RetentionPolicy);
   }

   public static TemplateDto ToTemplateDto(this Template template)
   {
      return new TemplateDto(
         template.Id,
         template.Key,
         template.Name,
         template.Type,
         template.RetentionPolicy,
         template.CreatedAt,
         template.CreatedBy,
         template.UpdatedAt,
         template.UpdatedBy,
         template.EmailTranslations.Select(t => t.ToEmailTemplateTranslationDto()).ToArray());
   }

   private static TemplateEmailTranslationDto ToEmailTemplateTranslationDto(this TemplateEmailTranslation translation)
   {
      return new TemplateEmailTranslationDto(
         translation.IsHtml,
         translation.Language,
         translation.Subject,
         translation.Body);
   }
}
