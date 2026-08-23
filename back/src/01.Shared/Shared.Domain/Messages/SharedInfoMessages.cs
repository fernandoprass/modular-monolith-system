using Myce.Response.Messages;

namespace Shared.Domain.Messages;

public class SuccessInfo : InformationMessage
{
   public SuccessInfo()
      : base(
         SharedTranslatedMessagesProvider.SuccessInfo,
         SharedTranslatedMessagesProvider.Instance.GetTranslations(SharedTranslatedMessagesProvider.SuccessInfo)) { }
}
