using Myce.Response.Messages;

namespace IAM.Domain.Messages;

public class PermissionDuplicateError : ErrorMessage
{
   public PermissionDuplicateError(string code)
      : base(
         IamTranslatedMessagesProvider.PermissionDuplicateError,
         IamTranslatedMessagesProvider.Instance.GetTranslations(IamTranslatedMessagesProvider.PermissionDuplicateError))
   {
      AddVariable(IamConst.Messages.Variables.Code, code);
   }
}

public class PermissionNotFoundInAssignmentError : ErrorMessage
{
   public PermissionNotFoundInAssignmentError()
      : base(
         IamTranslatedMessagesProvider.PermissionNotFoundInAssignmentError,
         IamTranslatedMessagesProvider.Instance.GetTranslations(IamTranslatedMessagesProvider.PermissionNotFoundInAssignmentError)) { }
}

public class PermissionsCannotBeUnassignedError : ErrorMessage
{
   public PermissionsCannotBeUnassignedError()
      : base(
         IamTranslatedMessagesProvider.PermissionsCannotBeUnassignedError,
         IamTranslatedMessagesProvider.Instance.GetTranslations(IamTranslatedMessagesProvider.PermissionsCannotBeUnassignedError)) { }
}
