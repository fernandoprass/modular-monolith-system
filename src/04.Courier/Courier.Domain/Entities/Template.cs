using Courier.Domain.Enums;
using Courier.Domain.ValueObjects;
using Shared.Domain.Entities;
using Shared.Domain.Enums;

namespace Courier.Domain.Entities;

public class Template : EntityAudited<Guid>
{
   private List<TemplateEmailTranslation> _emailTranslations = [];

   public string Key { get; private set; } = string.Empty;
   public string Name { get; private set; } = string.Empty;
   public TemplateType Type { get; private set; } = TemplateType.Email;
   public RetentionPolicy RetentionPolicy { get; private set; } = RetentionPolicy.Operational;

   public IReadOnlyCollection<TemplateEmailTranslation> EmailTranslations => _emailTranslations.AsReadOnly();

   private Template() { }

   public static Template Create(string key, string name, TemplateType type, RetentionPolicy retentionPolicy, Guid createdBy)
   {
      var now = DateTime.UtcNow;

      return new Template
      {
         Id = Guid.CreateVersion7(),
         Key = NormalizeKey(key),
         Name = name.Trim(),
         Type = type,
         RetentionPolicy = retentionPolicy,
         CreatedAt = now,
         CreatedBy = createdBy
      };
   }

   public void Update(string key, string name, TemplateType type, RetentionPolicy retentionPolicy, Guid updatedBy)
   {
      Key = NormalizeKey(key);
      Name = name.Trim();
      Type = type;
      RetentionPolicy = retentionPolicy;
      MarkUpdated(updatedBy);
   }

   public bool AddEmailTranslation(string language, string subject, string body, Guid updatedBy)
   {
      var normalizedLanguage = NormalizeLanguage(language);

      if (_emailTranslations.Any(t => t.Language == normalizedLanguage))
      {
         return false;
      }

      _emailTranslations.Add(TemplateEmailTranslation.Create(normalizedLanguage, subject, body));
      MarkUpdated(updatedBy);
      return true;
   }

   public bool UpdateEmailTranslation(string language, string subject, string body, Guid updatedBy)
   {
      var normalizedLanguage = NormalizeLanguage(language);
      var translation = _emailTranslations.SingleOrDefault(t => t.Language == normalizedLanguage);

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
      var removed = Type switch
      {
         TemplateType.Email => _emailTranslations.RemoveAll(t => t.Language == normalizedLanguage) > 0,
         _ => false
      };

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
