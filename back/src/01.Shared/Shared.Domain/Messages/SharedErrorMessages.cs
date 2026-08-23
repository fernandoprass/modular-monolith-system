using Myce.Response.Messages;

namespace Shared.Domain.Messages;

public class FailedToRecordDataError : ErrorMessage
{
   public FailedToRecordDataError()
      : base(
         SharedTranslatedMessagesProvider.FailedToRecordDataError,
         SharedTranslatedMessagesProvider.Instance.GetTranslations(SharedTranslatedMessagesProvider.FailedToRecordDataError)) { }
}

public class NotFoundError : ErrorMessage
{
   public NotFoundError()
      : base(
         SharedTranslatedMessagesProvider.NotFoundError,
         SharedTranslatedMessagesProvider.Instance.GetTranslations(SharedTranslatedMessagesProvider.NotFoundError)) { }

   public NotFoundError(string entity)
      : base(
         SharedTranslatedMessagesProvider.NotFoundDetailedError,
         SharedTranslatedMessagesProvider.Instance.GetTranslations(SharedTranslatedMessagesProvider.NotFoundDetailedError))
   {
      AddVariable(SharedConst.Message.Variable.Entity, entity);
   }
}

public class UnauthorizedAccessError : ErrorMessage
{
   public UnauthorizedAccessError()
      : base(
         SharedTranslatedMessagesProvider.UnauthorizedAccessError,
         SharedTranslatedMessagesProvider.Instance.GetTranslations(SharedTranslatedMessagesProvider.UnauthorizedAccessError)) { }
}

public class InvalidLanguageError : ErrorMessage
{
   public InvalidLanguageError(string language)
     : base(
        SharedTranslatedMessagesProvider.InvalidLanguageError,
        SharedTranslatedMessagesProvider.Instance.GetTranslations(SharedTranslatedMessagesProvider.InvalidLanguageError))
   {
      AddVariable(SharedConst.Message.Variable.Language, language);
   }
}
