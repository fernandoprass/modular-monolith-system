namespace Courier.Domain.ValueObjects;

public class TemplateTranslationNotification
{
   public string Title { get; private set; } = string.Empty;
   public string Message { get; private set; } = string.Empty;
   public string? ActionLink { get; private set; }


   private TemplateTranslationNotification() { }

   public static TemplateTranslationNotification Create(string title, string message, string? actionLink)
   {
      return new TemplateTranslationNotification
      {
         Title = title.Trim(),
         Message = message.Trim(),
         ActionLink = NormalizeActionLink(actionLink)
      };
   }

   public void Update(string title, string message, string? actionLink)
   {
      Title = title.Trim();
      Message = message.Trim();
      ActionLink = NormalizeActionLink(actionLink);
   }

   private static string? NormalizeActionLink(string? actionLink)
   {
      var normalized = actionLink?.Trim();
      return string.IsNullOrEmpty(normalized) ? null : normalized;
   }
}
