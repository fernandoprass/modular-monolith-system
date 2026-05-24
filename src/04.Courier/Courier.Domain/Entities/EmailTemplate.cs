using Courier.Domain.Enums;
using Courier.Domain.ValueObjects;
using Shared.Domain.Entities;

namespace Courier.Domain.Entities;

public class EmailTemplate : EntityAudited<Guid>
{
   private List<EmailTemplateTranslation> _translations = [];

   public string Key { get; private set; } = string.Empty;
   public string Name { get; private set; } = string.Empty;
   public EmailRetentionPolicy RetentionPolicy { get; private set; } = EmailRetentionPolicy.Operational;

   public IReadOnlyCollection<EmailTemplateTranslation> Translations => _translations.AsReadOnly();

   private EmailTemplate() { }

   public static EmailTemplate Create(string key, string name, EmailRetentionPolicy retentionPolicy, Guid createdBy)
   {
      var now = DateTime.UtcNow;

      return new EmailTemplate
      {
         Id = Guid.CreateVersion7(),
         Key = NormalizeKey(key),
         Name = name.Trim(),
         RetentionPolicy = retentionPolicy,
         CreatedAt = now,
         CreatedBy = createdBy
      };
   }

   public void Update(string key, string name, EmailRetentionPolicy retentionPolicy, Guid updatedBy)
   {
      Key = NormalizeKey(key);
      Name = name.Trim();
      RetentionPolicy = retentionPolicy;
      MarkUpdated(updatedBy);
   }

   public bool AddTranslation(string language, string subject, string body, Guid updatedBy)
   {
      var normalizedLanguage = NormalizeLanguage(language);

      if (_translations.Any(t => t.Language == normalizedLanguage))
      {
         return false;
      }

      _translations.Add(EmailTemplateTranslation.Create(normalizedLanguage, subject, body));
      MarkUpdated(updatedBy);
      return true;
   }

   public bool UpdateTranslation(string language, string subject, string body, Guid updatedBy)
   {
      var normalizedLanguage = NormalizeLanguage(language);
      var translation = _translations.SingleOrDefault(t => t.Language == normalizedLanguage);

      if (translation == null)
      {
         return false;
      }

      translation.Update(subject, body);
      MarkUpdated(updatedBy);
      return true;
   }

   public bool RemoveTranslation(string language, Guid updatedBy)
   {
      var normalizedLanguage = NormalizeLanguage(language);
      var removed = _translations.RemoveAll(t => t.Language == normalizedLanguage) > 0;

      if (removed)
      {
         MarkUpdated(updatedBy);
      }

      return removed;
   }

   private void MarkUpdated(Guid updatedBy)
   {
      UpdatedAt = DateTime.UtcNow;
      UpdatedBy = updatedBy;
   }

   private static string NormalizeKey(string key)
   {
      return key.ToLowerInvariant().Trim();
   }

   private static string NormalizeLanguage(string language)
   {
      return language.ToLowerInvariant().Trim();
   }
}
