using Shared.Domain;

namespace Courier.Domain.ValueObjects;

public class TemplateTranslation
{
   public string Language { get; private set; } = SharedConst.System.DefaultLanguage;
   public string Name { get; private set; } = string.Empty;

   public TemplateTranslationEmail? Email { get; private set; }
   public TemplateTranslationNotification? Notification { get; private set; }

   private TemplateTranslation() { }

   public static TemplateTranslation Create(
      string language,
      string name,
      TemplateTranslationEmail? email,
      TemplateTranslationNotification? notification)
   {
      return new TemplateTranslation
      {
         Language = LanguageOptions.Normalize(language),
         Name = name.Trim(),
         Email = email,
         Notification = notification
      };
   }

   public void Update(
      string name,
      TemplateTranslationEmail? email,
      TemplateTranslationNotification? notification)
   {
      Name = name.Trim();
      Email = email;
      Notification = notification;
   }
}

