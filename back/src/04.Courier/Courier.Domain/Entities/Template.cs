using Courier.Domain.Enums;
using Courier.Domain.ValueObjects;
using Shared.Domain;
using Shared.Domain.Entities;
using Shared.Domain.Enums;

namespace Courier.Domain.Entities;

public class Template : EntityAudited<Guid>
{
   private List<TemplateTranslation> _translations = [];

   public string Module { get; private set; } = string.Empty;
   public string Key { get; private set; } = string.Empty;
   public bool IsAllowingOptOut { get; private set; } = false;

   public NotificationSeverity Severity { get; private set; } = NotificationSeverity.Information;

   public RetentionPolicy RetentionPolicy { get; private set; } = RetentionPolicy.Operational;

   public IReadOnlyCollection<TemplateTranslation> Translations => _translations.AsReadOnly();

   private Template() { }

   public static Template Create(
      string module,
      string key,
      bool isAllowingOptOut,
      NotificationSeverity severity,
      RetentionPolicy retentionPolicy,
      Guid createdBy)
   {
      var now = DateTime.UtcNow;

      return new Template
      {
         Id = Guid.CreateVersion7(),
         Module = NormalizeModule(module),
         Key = NormalizeKey(key),
         IsAllowingOptOut = isAllowingOptOut,
         Severity = severity,
         RetentionPolicy = retentionPolicy,
         CreatedAt = now,
         CreatedBy = createdBy
      };
   }

   public void Update(
      string module,
      string key,
      bool isAllowingOptOut,
      NotificationSeverity severity,
      RetentionPolicy retentionPolicy,
      Guid updatedBy)
   {
      Module = NormalizeModule(module);
      Key = NormalizeKey(key);
      IsAllowingOptOut = isAllowingOptOut;
      Severity = severity;
      RetentionPolicy = retentionPolicy;
      MarkUpdated(updatedBy);
   }

   public bool AddTranslation(TemplateTranslation translation, Guid updatedBy)
   {
      if (_translations.Any(t => t.Language == translation.Language))
      {
         return false;
      }

      _translations.Add(translation);
      MarkUpdated(updatedBy);
      return true;
   }

   public bool UpdateTranslation(string language, TemplateTranslation updatedTranslation, Guid updatedBy)
   {
      var normalizedLanguage = LanguageOptions.Normalize(language);
      var translation = _translations.SingleOrDefault(t => t.Language == normalizedLanguage);

      if (translation == null)
      {
         return false;
      }

      translation.Update(
         updatedTranslation.Name,
         updatedTranslation.Email,
         updatedTranslation.Notification);
      MarkUpdated(updatedBy);
      return true;
   }

   public bool RemoveTranslation(string language, Guid updatedBy)
   {
      var normalizedLanguage = LanguageOptions.Normalize(language);
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

   private static string NormalizeModule(string module)
   {
      return module.ToLowerInvariant().Trim();
   }

   private static string NormalizeKey(string key)
   {
      return key.ToLowerInvariant().Trim();
   }
}
