using Myce.Response.Messages;

namespace IAM.Domain.Messages;

public class PermissionAssignedInfo : InformationMessage
{
   public PermissionAssignedInfo()
      : base(
         IamTranslatedMessagesProvider.PermissionAssignedSuccess,
         IamTranslatedMessagesProvider.Instance.GetTranslations(IamTranslatedMessagesProvider.PermissionAssignedSuccess))
   { }
}

public class PermissionUnassignedInfo : InformationMessage
{
   public PermissionUnassignedInfo()
      : base(
         IamTranslatedMessagesProvider.PermissionUnassignedSuccess,
         IamTranslatedMessagesProvider.Instance.GetTranslations(IamTranslatedMessagesProvider.PermissionUnassignedSuccess))
   { }
}