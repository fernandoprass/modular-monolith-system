using Myce.Response.Messages;

namespace Courier.Domain.Messages;

public class TemplateDuplicateKeyError : ErrorMessage
{
   public TemplateDuplicateKeyError(string module, string key)
      : base(
         CourierTranslatedMessagesProvider.TemplateDuplicateKeyError,
         CourierTranslatedMessagesProvider.Instance.GetTranslations(CourierTranslatedMessagesProvider.TemplateDuplicateKeyError))
   {
      AddVariable(CourierConst.Message.Variable.Module, module);
      AddVariable(CourierConst.Message.Variable.Key, key);
   }
}

public class TemplateTranslationAlreadyExistsError : ErrorMessage
{
   public TemplateTranslationAlreadyExistsError(string language)
      : base(
         CourierTranslatedMessagesProvider.TemplateTranslationAlreadyExistsError,
         CourierTranslatedMessagesProvider.Instance.GetTranslations(CourierTranslatedMessagesProvider.TemplateTranslationAlreadyExistsError))
   {
      AddVariable(CourierConst.Message.Variable.Language, language);
   }
}

public class TemplateTranslationNotFoundError : ErrorMessage
{
   public TemplateTranslationNotFoundError(string language)
      : base(
         CourierTranslatedMessagesProvider.TemplateTranslationNotFoundError,
         CourierTranslatedMessagesProvider.Instance.GetTranslations(CourierTranslatedMessagesProvider.TemplateTranslationNotFoundError))
   {
      AddVariable(CourierConst.Message.Variable.Language, language);
   }
}

public class TemplateChannelRequiredError : ErrorMessage
{
   public TemplateChannelRequiredError()
      : base(
         CourierTranslatedMessagesProvider.TemplateChannelRequiredError,
         CourierTranslatedMessagesProvider.Instance.GetTranslations(CourierTranslatedMessagesProvider.TemplateChannelRequiredError)) { }
}

public class TemplateEmailChannelNotFoundError : ErrorMessage
{
   public TemplateEmailChannelNotFoundError(string key, string language)
      : base(
         CourierTranslatedMessagesProvider.TemplateEmailChannelNotFoundError,
         CourierTranslatedMessagesProvider.Instance.GetTranslations(CourierTranslatedMessagesProvider.TemplateEmailChannelNotFoundError))
   {
      AddVariable(CourierConst.Message.Variable.Key, key);
      AddVariable(CourierConst.Message.Variable.Language, language);
   }
}

public class EmailTemplatePlaceholderMissingError : ErrorMessage
{
   public EmailTemplatePlaceholderMissingError(string placeholder)
      : base(
         CourierTranslatedMessagesProvider.EmailTemplatePlaceholderMissingError,
         CourierTranslatedMessagesProvider.Instance.GetTranslations(CourierTranslatedMessagesProvider.EmailTemplatePlaceholderMissingError))
   {
      AddVariable(CourierConst.Message.Variable.Placeholder, placeholder);
   }
}

public class TemplateLanguageNotFoundError : ErrorMessage
{
   public TemplateLanguageNotFoundError(string key, string language)
      : base(
         CourierTranslatedMessagesProvider.TemplateLanguageNotFoundError,
         CourierTranslatedMessagesProvider.Instance.GetTranslations(CourierTranslatedMessagesProvider.TemplateLanguageNotFoundError))
   {
      AddVariable(CourierConst.Message.Variable.Key, key);
      AddVariable(CourierConst.Message.Variable.Language, language);
   }
}

public class EmailDeliveryFailedError(string message) : ErrorMessage("EmailDeliveryFailedError", message)
{
}
