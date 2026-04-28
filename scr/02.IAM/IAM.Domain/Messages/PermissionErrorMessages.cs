using Myce.Response.Messages;

namespace IAM.Domain.Messages;

public class PermissionDuplicateCodeError : ErrorMessage
{
   public PermissionDuplicateCodeError(string code)
      : base("PermissionDuplicateCodeError", "The permission code '{code}' already exists.")
   {
      AddVariable("code", "code");
   }
}

public class PermissionNotFoundInAssignmentError : ErrorMessage
{
   public PermissionNotFoundInAssignmentError()
      : base("PermissionNotFoundInAssignmentError", "One or more permissions do not exist.") { }
}
