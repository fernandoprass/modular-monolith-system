namespace Courier.Domain.ValueObjects;

using System.Text.RegularExpressions;

public partial class TemplateTranslationEmail
{
   public bool IsHtml { get; private set; } = false;
   public string Subject { get; private set; } = string.Empty;
   public string Body { get; private set; } = string.Empty;

   private TemplateTranslationEmail() { }

   public static TemplateTranslationEmail Create(string subject, string body)
   {
      return new TemplateTranslationEmail
      {
         Subject = subject.Trim(),
         Body = body,
         IsHtml = GetIsHtml(body)
      };
   }

   public void Update(string subject, string body)
   {
      Subject = subject.Trim();
      Body = body;
      IsHtml = GetIsHtml(body);
   }

   [GeneratedRegex(@"<\s*[a-z][^>]*>", RegexOptions.IgnoreCase)]
   private static partial Regex HtmlDetectorRegex();

   private static bool GetIsHtml(string body)
   {
      return HtmlDetectorRegex().IsMatch(body);
   }
}

