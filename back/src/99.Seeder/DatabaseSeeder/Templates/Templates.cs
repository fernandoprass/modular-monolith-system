using Courier.Domain.Entities;
using Courier.Domain.Enums;
using Courier.Domain.Interfaces.Repositories;
using Courier.Domain.ValueObjects;
using Shared.Domain;
using Shared.Domain.Enums;

namespace DatabaseSeeder.Templates;

public class Templates(
   ITemplateRepository templateRepository,
   ITemplateWriteRepository templateWriteRepository)
{
   public async Task SeedAsync(CancellationToken cancellationToken = default)
   {
      foreach (var template in TemplatesIam.Templates)
      {
         await AddTemplateAsync(TemplatesIam.Module, template, cancellationToken);
      }
   }

   private async Task AddTemplateAsync(string module, TemplateSeed seed, CancellationToken cancellationToken)
   {
      var template = await templateRepository.GetByModuleAndKeyAsync(module, seed.Key, cancellationToken);

      if (template == null)
      {
         template = Template.Create(
            module,
            seed.Key,
            seed.AllowOptOut,
            seed.Severity,
            seed.RetentionPolicy,
            Guid.Empty);
         AddTranslation(
            template,
            LanguageOptions.English,
            seed.NameEn,
            seed.EmailSubjectEn,
            seed.EmailBodyEn,
            seed.NotificationTitleEn,
            seed.NotificationMessageEn);
         AddTranslation(
            template,
            LanguageOptions.PortugueseBrazil,
            seed.NamePt,
            seed.EmailSubjectPt,
            seed.EmailBodyPt,
            seed.NotificationTitlePt,
            seed.NotificationMessagePt);

         await templateWriteRepository.AddAsync(template, cancellationToken);
         Console.WriteLine($"Template: {seed.Key}");
         return;
      }

      var changed = false;

      changed |= AddTranslation(
         template,
         LanguageOptions.English,
         seed.NameEn,
         seed.EmailSubjectEn,
         seed.EmailBodyEn,
         seed.NotificationTitleEn,
         seed.NotificationMessageEn);
      changed |= AddTranslation(
         template,
         LanguageOptions.PortugueseBrazil,
         seed.NamePt,
         seed.EmailSubjectPt,
         seed.EmailBodyPt,
         seed.NotificationTitlePt,
         seed.NotificationMessagePt);

      if (changed)
      {
         await templateWriteRepository.UpdateAsync(template, cancellationToken);
         Console.WriteLine($"Template updated: {seed.Key}");
      }
   }

   private static bool AddTranslation(
      Template template,
      string language,
      string name,
      string subject,
      string body,
      string notificationTitle,
      string notificationMessage)
   {
      var translation = TemplateTranslation.Create(
         language,
         name,
         TemplateTranslationEmail.Create(subject, body),
         TemplateTranslationNotification.Create(notificationTitle, notificationMessage, null));

      return template.AddTranslation(translation, Guid.Empty);
   }

   public sealed record TemplateSeed(
      string Key,
      bool AllowOptOut,
      NotificationSeverity Severity,
      RetentionPolicy RetentionPolicy,
      string NameEn,
      string EmailSubjectEn,
      string EmailBodyEn,
      string NotificationTitleEn,
      string NotificationMessageEn,
      string NamePt,
      string EmailSubjectPt,
      string EmailBodyPt,
      string NotificationTitlePt,
      string NotificationMessagePt);
}
