namespace Courier.Domain.ValueObjects;

public class EmailTemplateTranslation
{
   public string Language { get; private set; } = string.Empty;
   public string Name { get; private set; } = string.Empty;
   public string Subject { get; private set; } = string.Empty;
   public string Body { get; private set; } = string.Empty;

   private EmailTemplateTranslation() { }

   public static EmailTemplateTranslation Create(string language, string name, string subject, string body)
   {
      return new EmailTemplateTranslation
      {
         Language = language.ToLowerInvariant().Trim(),
         Name = name.Trim(),
         Subject = subject.Trim(),
         Body = body
      };
   }

   public void Update(string name, string subject, string body)
   {
      Name = name.Trim();
      Subject = subject.Trim();
      Body = body;
   }
}
