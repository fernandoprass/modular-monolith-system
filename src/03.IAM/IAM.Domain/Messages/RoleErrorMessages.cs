using Myce.Response.Messages;

namespace IAM.Domain.Messages
{
   public class RoleDuplicateNameError : ErrorMessage
   {
      public RoleDuplicateNameError(string name) 
         : base("RoleDuplicateNameError", "A role with the name '{name}' already exists.")
      {
         AddVariable("name", name);
      }
   }

   public class RolesCannotBeAssignedError : ErrorMessage
   {
      public RolesCannotBeAssignedError() : base("RolesCannotBeAssignedError", "Roles cannot be assigned. One or more roles added to the list are inactive or belong to another organization.") { }
   }

   public class RolesCannotBeUnassignedError : ErrorMessage
   {
      public RolesCannotBeUnassignedError() : base("RolesCannotBeUnassignedError", "Roles cannot be unassigned. One or more roles are not assigned to the user.") { }
   }

   public class RolesInvalidExpirationError : ErrorMessage
   {
      public RolesInvalidExpirationError() : base("RolesInvalidExpirationError", "Expire date should be in the future.") { }
   }
}
