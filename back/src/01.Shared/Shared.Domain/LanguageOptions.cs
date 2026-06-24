namespace Shared.Domain;

/// <summary>
/// Supported language codes for emails, notifications, and user preferences.
/// </summary>
public static class LanguageOptions
{
   public const string English = "en";
   public const string PortugueseBrazil = "pt-BR";
   public const string Spanish = "es";

   public static readonly IReadOnlyList<string> AllowedLanguages =
   [
      English,
      PortugueseBrazil,
      Spanish
   ];

   /// <summary>
   /// Checks if the provided language code is allowed.
   /// </summary>
   public static bool IsSupported(string language)
   {
      if (string.IsNullOrWhiteSpace(language))
      {
         return false;
      }

      var normalizedLanguage = Normalize(language);
      return AllowedLanguages.Any(allowedLanguage => Normalize(allowedLanguage) == normalizedLanguage);
   }

   /// <summary>
   /// Normalizes a language code.
   /// </summary>
   public static string Normalize(string language)
   {
      if (string.IsNullOrWhiteSpace(language))
      {
         return English;
      }

      var parts = language.Trim().Split('-');

      if (parts.Length == 1)
         return parts[0].ToLowerInvariant();

      // If it has region ("pt-br"), normalize to lower case + "-" upper case ("pt-BR")
      return $"{parts[0].ToLowerInvariant()}-{parts[1].ToUpperInvariant()}";
   }
}
