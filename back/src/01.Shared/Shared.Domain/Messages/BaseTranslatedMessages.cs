using System.Globalization;

namespace Shared.Domain.Messages
{
   public abstract class BaseTranslatedMessages
   {
      protected readonly Dictionary<string, Dictionary<string, string>> _translations = new();

      public void AddTranslation(string errorCode, string language, string message)
      {
         //todo convert to culture info and use it to format the message
         //CultureInfo culture = CultureInfo.InvariantCulture;
         _translations.TryAdd(errorCode, []);
         _translations[errorCode].TryAdd(language, message);
      }
   }
}
