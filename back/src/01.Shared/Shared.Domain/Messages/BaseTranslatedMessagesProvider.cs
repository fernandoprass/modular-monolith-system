using Myce.Response.Messages;

namespace Shared.Domain.Messages
{
   public abstract class BaseTranslatedMessagesProvider
   {
      private readonly Dictionary<string, Dictionary<string, string>> _translations = [];

      public void AddTranslation(string code, string language, string message)
      {
         _translations.TryAdd(code, []);
         _translations[code][LanguageOptions.Normalize(language)] = message;
      }

      public string GetTranslation(string code, string language)
      {
         if (!_translations.TryGetValue(code, out var translations))
         {
            return string.Empty;
         }

         var normalizedLanguage = LanguageOptions.Normalize(language);
         if (translations.TryGetValue(normalizedLanguage, out string? message))
         {
            return message;
         }

         return translations.TryGetValue(LanguageOptions.English, out string? defaultMessage)
            ? defaultMessage
            : string.Empty;
      }

      public Dictionary<string, string> GetTranslations(string code)
      {
         if (_translations.TryGetValue(code, out var translations))
         {
            return new Dictionary<string, string>(translations);
         }

         throw new KeyNotFoundException($"Translation code '{code}' was not found.");
      }

      public ErrorMessage Error(string code)
      {
         return new ErrorMessage(code, GetTranslations(code));
      }

      public InformationMessage Info(string code)
      {
         return new InformationMessage(code, GetTranslations(code));
      }

      public WarningMessage Warning(string code)
      {
         return new WarningMessage(code, GetTranslations(code));
      }
   }
}
