using Myce.Response.Messages;

namespace IAM.Domain.Messages;

public class RoleAssignedInfo : InformationMessage
{
   public RoleAssignedInfo()
      : base(
         IamTranslatedMessagesProvider.RoleAssignedSuccess,
         IamTranslatedMessagesProvider.Instance.GetTranslations(IamTranslatedMessagesProvider.RoleAssignedSuccess))
   { }
}

public class RoleUnassignedInfo : InformationMessage
{
   public RoleUnassignedInfo()
      : base(
         IamTranslatedMessagesProvider.RoleUnassignedSuccess,
         IamTranslatedMessagesProvider.Instance.GetTranslations(IamTranslatedMessagesProvider.RoleUnassignedSuccess))
   { }
}