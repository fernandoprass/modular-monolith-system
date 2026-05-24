using Courier.Domain.DTOs.Responses;
using Courier.Domain.Entities;
using Courier.Domain.ValueObjects;

namespace Courier.Domain.Mappers;

public static class EmailTemplateMappers
{
   public static EmailTemplateDto ToEmailTemplateDto(this EmailTemplate template)
   {
      return new EmailTemplateDto(
         template.Id,
         template.Key,
         template.Name,
         template.RetentionPolicy,
         template.CreatedAt,
         template.CreatedBy,
         template.UpdatedAt,
         template.UpdatedBy,
         template.Translations.Select(t => t.ToEmailTemplateTranslationDto()).ToArray());
   }

   private static EmailTemplateTranslationDto ToEmailTemplateTranslationDto(this EmailTemplateTranslation translation)
   {
      return new 
         EmailTemplateTranslationDto(
         translation.IsHtml,
         translation.Language,
         translation.Subject,
         translation.Body);
   }
}
