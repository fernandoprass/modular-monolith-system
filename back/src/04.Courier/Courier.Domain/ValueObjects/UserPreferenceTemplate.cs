namespace Courier.Domain.ValueObjects;

public class UserPreferenceTemplate
{
   public string Module { get; private set; } = string.Empty;
   public string TemplateKey { get; private set; } = string.Empty;

   private UserPreferenceTemplate() { }

   public UserPreferenceTemplate(string module, string templateKey)
   {
      Module = module;
      TemplateKey = templateKey;
   }
}
