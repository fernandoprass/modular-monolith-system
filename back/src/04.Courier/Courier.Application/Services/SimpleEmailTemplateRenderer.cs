using Courier.Application.Contracts;
using Courier.Domain.Messages;
using Myce.Response;
using System.Net;
using System.Text.RegularExpressions;

namespace Courier.Application.Services;

public partial class SimpleEmailTemplateRenderer : IEmailTemplateRenderer
{
   public Result<string> Render(
      string template,
      IReadOnlyDictionary<string, string> values,
      bool htmlEncodeValues = false)
   {
      var missingPlaceholder = PlaceholderRegex()
         .Matches(template)
         .Select(match => match.Groups["key"].Value.Trim())
         .FirstOrDefault(key => !values.ContainsKey(key));

      if (!string.IsNullOrWhiteSpace(missingPlaceholder))
      {
         return Result<string>.Failure(new EmailTemplatePlaceholderMissingError(missingPlaceholder));
      }

      var rendered = PlaceholderRegex().Replace(template, match =>
      {
         var key = match.Groups["key"].Value.Trim();
         var value = values[key];

         return htmlEncodeValues
            ? WebUtility.HtmlEncode(value)
            : value;
      });

      return Result<string>.Success(rendered);
   }

   [GeneratedRegex(@"\{\{\s*(?<key>[a-zA-Z0-9_.-]+)\s*\}\}")]
   private static partial Regex PlaceholderRegex();
}
