using Myce.Response.Messages;

namespace Courier.Domain.Messages;

public class EmailTemplateDuplicateKeyError : ErrorMessage
{
   public EmailTemplateDuplicateKeyError(string key)
      : base("EmailTemplateDuplicateKeyError", "Email template key already exists: {key}.")
   {
      AddVariable("key", key);
   }
}

public class EmailTemplateTranslationAlreadyExistsError : ErrorMessage
{
   public EmailTemplateTranslationAlreadyExistsError(string language)
      : base("EmailTemplateTranslationAlreadyExistsError", "Translation already exists for language: {language}.")
   {
      AddVariable("language", language);
   }
}

public class EmailTemplateTranslationNotFoundError : ErrorMessage
{
   public EmailTemplateTranslationNotFoundError(string language)
      : base("EmailTemplateTranslationNotFoundError", "Translation not found for language: {language}.")
   {
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

public class EmailTemplateLanguageNotFoundError : ErrorMessage
{
   public EmailTemplateLanguageNotFoundError(string key, string language)
      : base("EmailTemplateLanguageNotFoundError", "Email template {key} does not have translation for language: {language}.")
   {
      AddVariable("key", key);
      AddVariable("language", language);
   }
}

public class EmailDeliveryFailedError(string message) : ErrorMessage("EmailDeliveryFailedError", message)
{
}
