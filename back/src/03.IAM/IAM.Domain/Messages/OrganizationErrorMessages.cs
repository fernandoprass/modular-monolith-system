using Myce.Response.Messages;

namespace IAM.Domain.Messages;

public static class OrganizationErrorMessages
{
   private const string Code = "code";

   public static ErrorMessage DuplicateCode(string code)
   {
      var error = IamTranslatedMessagesProvider.Instance.Error(IamTranslatedMessagesProvider.OrganizationDuplicateError);
      error.AddVariable(Code, code);
      return error;
   }

   public static ErrorMessage Forbidden()
   {
      return IamTranslatedMessagesProvider.Instance.Error(IamTranslatedMessagesProvider.OrganizationForbiddenError);
   }

   public static ErrorMessage InvalidCodeFormat()
   {
      return IamTranslatedMessagesProvider.Instance.Error(IamTranslatedMessagesProvider.OrganizationInvalidCodeFormatError);
   }

   public static ErrorMessage InvalidType()
   {
      return IamTranslatedMessagesProvider.Instance.Error(IamTranslatedMessagesProvider.OrganizationInvalidTypeError);
   }
}
