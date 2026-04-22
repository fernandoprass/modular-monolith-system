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

   public class RoleCannotUpdateDefaultError : ErrorMessage
   {
      public RoleCannotUpdateDefaultError() : base("RoleCannotUpdateDefaultError", "System default roles cannot be updated.") { }
   }
}
