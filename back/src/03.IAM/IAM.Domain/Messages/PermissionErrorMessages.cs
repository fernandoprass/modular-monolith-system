using Myce.Response.Messages;

namespace IAM.Domain.Messages;

public class PermissionDuplicateError : ErrorMessage
{
   public PermissionDuplicateError(string code)
      : base(
         IamTranslatedMessagesProvider.PermissionDuplicateCodeError,
         IamTranslatedMessagesProvider.Instance.GetTranslations(IamTranslatedMessagesProvider.PermissionDuplicateCodeError))
   {
      AddVariable(IamConst.Message.Variable.Code, code);
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
