using Myce.Response.Messages;

namespace IAM.Domain.Messages
{
   public class RoleDuplicateNameError : ErrorMessage
   {
      public RoleDuplicateNameError(string name)
         : base(
            IamTranslatedMessagesProvider.RoleDuplicateNameError,
            IamTranslatedMessagesProvider.Instance.GetTranslations(IamTranslatedMessagesProvider.RoleDuplicateNameError))
      {
         AddVariable(IamConst.Message.Variable.Name, name);
      }
   }

   public class RolesCannotBeAssignedError : ErrorMessage
   {
      public RolesCannotBeAssignedError()
         : base(
            IamTranslatedMessagesProvider.RolesCannotBeAssignedError,
            IamTranslatedMessagesProvider.Instance.GetTranslations(IamTranslatedMessagesProvider.RolesCannotBeAssignedError)) { }
   }

   public class RolesCannotBeUnassignedError : ErrorMessage
   {
      public RolesCannotBeUnassignedError()
         : base(
            IamTranslatedMessagesProvider.RolesCannotBeUnassignedError,
            IamTranslatedMessagesProvider.Instance.GetTranslations(IamTranslatedMessagesProvider.RolesCannotBeUnassignedError)) { }
   }

   public class RolesInvalidStartDateError : ErrorMessage
   {
      public RolesInvalidStartDateError()
         : base(
            IamTranslatedMessagesProvider.RolesInvalidStartDateError,
            IamTranslatedMessagesProvider.Instance.GetTranslations(IamTranslatedMessagesProvider.RolesInvalidStartDateError)) { }
   }

   public class RolesInvalidExpirationError : ErrorMessage
   {
      public RolesInvalidExpirationError()
         : base(
            IamTranslatedMessagesProvider.RolesInvalidExpirationError,
            IamTranslatedMessagesProvider.Instance.GetTranslations(IamTranslatedMessagesProvider.RolesInvalidExpirationError)) { }
   }
}
