using Shared.Application.Contracts;
using Shared.Domain;
using GanssHtmlSanitizer = Ganss.Xss.HtmlSanitizer;

namespace Shared.Application.Security;

internal sealed class HtmlSanitizerService : IHtmlSanitizer
{
   private readonly GanssHtmlSanitizer _sanitizer = CreateSanitizer();

   public string Sanitize(string html)
   {
      return string.IsNullOrEmpty(html)
         ? string.Empty
         : _sanitizer.Sanitize(html);
   }

   public bool IsSafeUrl(string url)
   {
      if (string.IsNullOrWhiteSpace(url))
      {
         return false;
      }

      var trimmedUrl = url.Trim();

      if (trimmedUrl.StartsWith("//", StringComparison.Ordinal))
      {
         return false;
      }

      if (trimmedUrl.StartsWith("/", StringComparison.Ordinal)
          || trimmedUrl.StartsWith("#", StringComparison.Ordinal))
      {
         return true;
      }

      return Uri.TryCreate(trimmedUrl, UriKind.Absolute, out var uri)
             && SharedConst.HtmlSanitizer.AllowedSchemes.Contains(uri.Scheme, StringComparer.OrdinalIgnoreCase);
   }

   private static GanssHtmlSanitizer CreateSanitizer()
   {
      var sanitizer = new GanssHtmlSanitizer();

      sanitizer.AllowedTags.Clear();
      sanitizer.AllowedAttributes.Clear();
      sanitizer.AllowedSchemes.Clear();

      foreach (var tag in SharedConst.HtmlSanitizer.AllowedTags)
      {
         sanitizer.AllowedTags.Add(tag);
      }

      foreach (var attribute in SharedConst.HtmlSanitizer.AllowedAttributes)
      {
         sanitizer.AllowedAttributes.Add(attribute);
      }

      foreach (var scheme in SharedConst.HtmlSanitizer.AllowedSchemes)
      {
         sanitizer.AllowedSchemes.Add(scheme);
      }

      foreach (var attribute in SharedConst.HtmlSanitizer.UriAttributes)
      {
         sanitizer.UriAttributes.Add(attribute);
      }

      return sanitizer;
   }
}
