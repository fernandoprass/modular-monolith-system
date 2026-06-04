using Myce.Extensions;

namespace Shared.Domain;

/// <summary>
/// Supported language codes for emails, notifications, and user preferences.
/// </summary>
public static class LanguageOptions
{
   public const string English = "en";
   public const string PortugueseBrazil = "pt-br";
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
      return !string.IsNullOrEmpty(language) && AllowedLanguages.Contains(language.Trim().ToLowerInvariant());
   }

   /// <summary>
   /// Normalizes a language code to lowercase and trims whitespace.
   /// </summary>
   public static string Normalize(string language)
   {
      return language.Trim().IsNullOrEmpty() ? English : language.Trim().ToLowerInvariant();
   }
}
