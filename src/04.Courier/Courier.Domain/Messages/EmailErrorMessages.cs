using Myce.Response.Messages;

namespace Courier.Domain.Messages;

public class EmailInvalidDateRangeError : ErrorMessage
{
   public EmailInvalidDateRangeError()
      : base("EmailInvalidDateRangeError", "DateFrom must be before DateTo.") { }
}

public class EmailInvalidPageNumberError : ErrorMessage
{
   public EmailInvalidPageNumberError()
      : base("EmailInvalidPageNumberError", "PageNumber must be greater than zero.") { }
}

public class EmailInvalidPageSizeError : ErrorMessage
{
   public EmailInvalidPageSizeError()
      : base("EmailInvalidPageSizeError", "PageSize must be greater than zero.") { }
}

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
