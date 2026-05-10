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

public class PermissionsCannotBeUnassignedError : ErrorMessage
{
   public PermissionsCannotBeUnassignedError()
      : base("PermissionsCannotBeUnassignedError", "Permissions cannot be unassigned. One or more permissions are not assigned to the role.") { }
}
