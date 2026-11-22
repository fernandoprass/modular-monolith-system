using Myce.Response.Messages;

namespace Shared.Domain.Messages;

internal class LookupInvalidTranslationsError : ErrorMessage
{
   public LookupInvalidTranslationsError()
      : base(
         SharedTranslatedMessagesProvider.LookupInvalidTranslationsError,
         SharedTranslatedMessagesProvider.Instance.GetTranslations(SharedTranslatedMessagesProvider.LookupInvalidTranslationsError)) { }
}

internal class LookupInvalidParentError : ErrorMessage
{
   public LookupInvalidParentError()
      : base(
         SharedTranslatedMessagesProvider.LookupInvalidParentError,
         SharedTranslatedMessagesProvider.Instance.GetTranslations(SharedTranslatedMessagesProvider.LookupInvalidParentError)) { }
}

internal class LookupDuplicateCodeError : ErrorMessage
{
   public LookupDuplicateCodeError()
      : base(
         SharedTranslatedMessagesProvider.LookupDuplicateCodeError,
         SharedTranslatedMessagesProvider.Instance.GetTranslations(SharedTranslatedMessagesProvider.LookupDuplicateCodeError)) { }
}

internal class LookupInvalidSearchError : ErrorMessage
{
   public LookupInvalidSearchError()
      : base(
         SharedTranslatedMessagesProvider.LookupInvalidSearchError,
         SharedTranslatedMessagesProvider.Instance.GetTranslations(SharedTranslatedMessagesProvider.LookupInvalidSearchError)) { }
}

internal class LookupInvalidPageError : ErrorMessage
{
   public LookupInvalidPageError()
      : base(
         SharedTranslatedMessagesProvider.LookupInvalidPageError,
         SharedTranslatedMessagesProvider.Instance.GetTranslations(SharedTranslatedMessagesProvider.LookupInvalidPageError)) { }
}
