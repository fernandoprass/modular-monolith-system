using Myce.Response.Messages;

namespace IAM.Domain.Messages.Errors
{
   public class UnauthorizedError : ErrorMessage
   {
      public UnauthorizedError() : base("UnauthorizedError", "Invalid email or password.") { }
   }

   public class DuplicateRoleNameError : ErrorMessage
   {
      public DuplicateRoleNameError(string name) : base("DuplicateRoleNameError", "A role with the name '{name}' already exists.")
      {
         AddVariable("name", name);
      }
   }

   public class CannotUpdateDefaultRoleError : ErrorMessage
   {
      public CannotUpdateDefaultRoleError() : base("CannotUpdateDefaultRoleError", "System default roles cannot be updated.") { }
   }
}
