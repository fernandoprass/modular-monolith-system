namespace Courier.Domain.ValueObjects;

using System.Text.RegularExpressions;

public class EmailTemplateTranslation
{
   public bool IsHtml { get; private set; } = false;
   public string Language { get; private set; } = string.Empty;
   public string Subject { get; private set; } = string.Empty;
   public string Body { get; private set; } = string.Empty;

   private EmailTemplateTranslation() { }

   public static EmailTemplateTranslation Create(string language, string subject, string body)
   {
      return new EmailTemplateTranslation
      {
         Language = language.ToLowerInvariant().Trim(),
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

   private static bool GetIsHtml(string body)
   {
      return Regex.IsMatch(body, @"<\s*[a-z][^>]*>", RegexOptions.IgnoreCase);
   }
}
