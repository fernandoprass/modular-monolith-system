using Myce.Response.Messages;

namespace Shared.Domain.Messages;

public class SuccessInfo : InformationMessage
{
   public SuccessInfo()
      : base(
         SharedTranslatedMessagesProvider.SuccessInfo,
         SharedTranslatedMessagesProvider.Instance.GetTranslations(SharedTranslatedMessagesProvider.SuccessInfo)) { }
}

public abstract class CrudSuccessInfo : InformationMessage
{
   protected CrudSuccessInfo(string code, IReadOnlyDictionary<string, string> entityTranslations)
      : base(
         code,
         SharedTranslatedMessagesProvider.Instance.GetTranslations(code))
   {
      foreach (var translation in entityTranslations)
      {
         AddVariable(translation.Key, SharedConst.Message.Variable.Entity, translation.Value);
      }
   }
}

public class CreatedSuccessInfo(IReadOnlyDictionary<string, string> entityTranslations)
   : CrudSuccessInfo(SharedTranslatedMessagesProvider.CrudCreatedSuccessInfo, entityTranslations) {}

public class UpdatedSuccessInfo(IReadOnlyDictionary<string, string> entityTranslations) 
   : CrudSuccessInfo(SharedTranslatedMessagesProvider.CrudUpdatedSuccessInfo, entityTranslations) {}

public class DeletedSuccessInfo(IReadOnlyDictionary<string, string> entityTranslations) 
   : CrudSuccessInfo(SharedTranslatedMessagesProvider.CrudDeletedSuccessInfo, entityTranslations) {}
