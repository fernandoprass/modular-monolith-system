using Myce.Response.Messages;

namespace IAM.Domain.Messages;

public class PermissionDuplicateCodeError : ErrorMessage
{
   public PermissionDuplicateCodeError(string code)
      : base(
         IamTranslatedMessagesProvider.PermissionDuplicateError,
         IamTranslatedMessagesProvider.Instance.GetTranslations(IamTranslatedMessagesProvider.PermissionDuplicateError))
   {
      AddVariable("code", code);
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
