using Myce.Response.Messages;

namespace Shared.Domain.Messages;

public class SuccessInfo : InformationMessage
{
   public SuccessInfo()
      : base(
         SharedTranslatedMessagesProvider.SuccessInfo,
         SharedTranslatedMessagesProvider.Instance.GetTranslations(SharedTranslatedMessagesProvider.SuccessInfo)) { }
}

public class CrudSuccessInfo : InformationMessage
{
   public CrudSuccessInfo(CrudOperation operation, string entityKey)
      : base(
         GetCode(operation),
         SharedTranslatedMessagesProvider.Instance.GetTranslations(GetCode(operation)))
   {
      foreach (var translation in SharedTranslatedMessagesProvider.Instance.GetVariableTranslations(entityKey))
      {
         AddVariable(translation.Key, SharedConst.Message.Variable.Entity, translation.Value);
      }
   }

   private static string GetCode(CrudOperation operation)
   {
      return operation switch
      {
         CrudOperation.Created => SharedTranslatedMessagesProvider.CrudCreatedSuccess,
         CrudOperation.Updated => SharedTranslatedMessagesProvider.CrudUpdatedSuccess,
         CrudOperation.Deleted => SharedTranslatedMessagesProvider.CrudDeletedSuccess,
         _ => throw new ArgumentOutOfRangeException(nameof(operation), operation, null)
      };
   }
}

public enum CrudOperation
{
   Created,
   Updated,
   Deleted
}
