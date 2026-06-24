using Myce.Response.Messages;

namespace Courier.Domain.Messages;

public class TemplateDuplicateKeyError : ErrorMessage
{
   public TemplateDuplicateKeyError(string module, string key)
      : base("TemplateDuplicateKeyError", "Template key already exists in module {module}: {key}.")
   {
      AddVariable("module", module);
      AddVariable("key", key);
   }
}

public class TemplateTranslationAlreadyExistsError : ErrorMessage
{
   public TemplateTranslationAlreadyExistsError(string language)
      : base("TemplateTranslationAlreadyExistsError", "Translation already exists for language: {language}.")
   {
      AddVariable("language", language);
   }
}

public class TemplateTranslationNotFoundError : ErrorMessage
{
   public TemplateTranslationNotFoundError(string language)
      : base("TemplateTranslationNotFoundError", "Translation not found for language: {language}.")
   {
      AddVariable("language", language);
   }
}

public class TemplateChannelRequiredError : ErrorMessage
{
   public TemplateChannelRequiredError()
      : base("TemplateChannelRequiredError", "At least one template channel is required.") { }
}

public class TemplateEmailChannelNotFoundError : ErrorMessage
{
   public TemplateEmailChannelNotFoundError(string key, string language)
      : base("TemplateEmailChannelNotFoundError", "Template {key} does not have email content for language: {language}.")
   {
      AddVariable("key", key);
      AddVariable("language", language);
   }
}

public class EmailTemplatePlaceholderMissingError : ErrorMessage
{
   public EmailTemplatePlaceholderMissingError(string placeholder)
      : base("EmailTemplatePlaceholderMissingError", "Email template placeholder is missing: {placeholder}.")
   {
      AddVariable("placeholder", placeholder);
   }
}

public class TemplateLanguageNotFoundError : ErrorMessage
{
   public TemplateLanguageNotFoundError(string key, string language)
      : base("TemplateLanguageNotFoundError", "Email template {key} does not have translation for language: {language}.")
   {
      AddVariable("key", key);
      AddVariable("language", language);
   }
}

public class EmailDeliveryFailedError(string message) : ErrorMessage("EmailDeliveryFailedError", message)
{
}
