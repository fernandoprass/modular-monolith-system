using Courier.Domain.ValueObjects;
using Shared.Domain.Entities;

namespace Courier.Domain.Entities
{
   public class UserPreference : Entity
   {
      private List<UserPreferenceTemplate> _disabledEmailTemplates = new();
      private List<UserPreferenceTemplate> _disabledNotificationTemplates = new();

      public Guid UserId { get; private set; }
      public bool IsGlobalEmailEnabled { get; private set; } = true;
      public bool IsGlobalNotificationEnabled { get; private set; } = true;
      public DateTime CreatedAt { get; private set; }
      public DateTime? UpdatedAt { get; private set; }
      public IReadOnlyCollection<UserPreferenceTemplate> DisabledEmailTemplates => _disabledEmailTemplates.AsReadOnly();
      public IReadOnlyCollection<UserPreferenceTemplate> DisabledNotificationTemplates => _disabledNotificationTemplates.AsReadOnly();

      private UserPreference() { }

      public static UserPreference CreateDefault(Guid userId)
      {
         return new UserPreference
         {
            Id = Guid.CreateVersion7(),
            UserId = userId,
            IsGlobalEmailEnabled = true,
            IsGlobalNotificationEnabled = true,
            CreatedAt = DateTime.UtcNow
         };
      }

      public void UpdateGlobalChannels(bool emailEnabled, bool notificationEnabled)
      {
         IsGlobalEmailEnabled = emailEnabled;
         IsGlobalNotificationEnabled = notificationEnabled;
         UpdatedAt = DateTime.UtcNow;
      }

      public void ReplaceTemplatePreferences(
         IEnumerable<UserPreferenceTemplate> disabledEmailTemplates,
         IEnumerable<UserPreferenceTemplate> disabledNotificationTemplates)
      {
         _disabledEmailTemplates.Clear();
         _disabledEmailTemplates.AddRange(disabledEmailTemplates.Select(t =>
            new UserPreferenceTemplate(Normalize(t.Module), Normalize(t.TemplateKey))));

         _disabledNotificationTemplates.Clear();
         _disabledNotificationTemplates.AddRange(disabledNotificationTemplates.Select(t =>
            new UserPreferenceTemplate(Normalize(t.Module), Normalize(t.TemplateKey))));

         UpdatedAt = DateTime.UtcNow;
      }

      public void DisableEmailTemplatePreference(string module, string templateKey)
      {
         var exists = FindTemplate(_disabledEmailTemplates, module, templateKey);

         if (exists == null)
         {
            _disabledEmailTemplates.Add(new UserPreferenceTemplate(Normalize(module), Normalize(templateKey)));
            UpdatedAt = DateTime.UtcNow;
         }
      }

      public void DisableNotificationTemplatePreference(string module, string templateKey)
      {
         var exists = FindTemplate(_disabledNotificationTemplates, module, templateKey);

         if (exists == null)
         {
            _disabledNotificationTemplates.Add(new UserPreferenceTemplate(Normalize(module), Normalize(templateKey)));
            UpdatedAt = DateTime.UtcNow;
         }
      }

      public bool IsEmailEnabledForTemplate(string module, string templateKey)
      {
         return IsGlobalEmailEnabled && FindTemplate(_disabledEmailTemplates, module, templateKey) == null;
      }

      public bool IsNotificationEnabledForTemplate(string module, string templateKey)
      {
         return IsGlobalNotificationEnabled && FindTemplate(_disabledNotificationTemplates, module, templateKey) == null;
      }

      private UserPreferenceTemplate? FindTemplate(List<UserPreferenceTemplate> sourceList, string module, string templateKey)
      {
         return sourceList.Find(p =>
             p.Module.Equals(module, StringComparison.OrdinalIgnoreCase) &&
             p.TemplateKey.Equals(templateKey, StringComparison.OrdinalIgnoreCase));
      }

      private static string Normalize(string value)
      {
         return value.ToLowerInvariant().Trim();
      }
   }
}

