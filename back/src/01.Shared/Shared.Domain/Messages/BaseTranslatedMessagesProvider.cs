using Myce.Response.Messages;

namespace Shared.Domain.Messages
{
   public abstract class BaseTranslatedMessagesProvider
   {
      //text translations: error code, language, message
      private readonly Dictionary<string, Dictionary<string, string>> _textTranslations = [];

      //variable translations: name, language, value
      private readonly Dictionary<string, Dictionary<string, string>> _variableTranslations = [];

      public void AddTranslation(string code, string language, string message)
      {
         _textTranslations.TryAdd(code, []);
         _textTranslations[code][LanguageOptions.Normalize(language)] = message;
      }

      public void AddVariableTranslation(string name, string language, string value)
      {
         _variableTranslations.TryAdd(name, []);
         _variableTranslations[name][LanguageOptions.Normalize(language)] = value;
      }

      public Dictionary<string, string> GetVariableTranslations(string name)
      {
         if (_variableTranslations.TryGetValue(name, out var translations))
         {
            return new Dictionary<string, string>(translations);
         }

         throw new KeyNotFoundException($"Variable translation '{name}' was not found.");
      }

      public string GetTranslation(string code, string language)
      {
         if (!_textTranslations.TryGetValue(code, out var translations))
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
         if (_textTranslations.TryGetValue(code, out var translations))
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

      public CreatedSuccessInfo CreatedSuccess(string entityKey)
      {
         return new CreatedSuccessInfo(GetVariableTranslations(entityKey));
      }

      public UpdatedSuccessInfo UpdatedSuccess(string entityKey)
      {
         return new UpdatedSuccessInfo(GetVariableTranslations(entityKey));
      }

      public DeletedSuccessInfo DeletedSuccess(string entityKey)
      {
         return new DeletedSuccessInfo(GetVariableTranslations(entityKey));
      }
   }
}
