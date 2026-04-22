using Myce.Response.Messages;

namespace IAM.Domain.Messages;

public class OrganizationDuplicateCodeError : ErrorMessage
{
   public OrganizationDuplicateCodeError(string code)
       : base("OrganizationDuplicateCodeError", "The organization code '{code}' already exists.")
   {
      AddVariable("code", code);
   }
}

public class OrganizationInvalidCodeFormatError : ErrorMessage
{
   public OrganizationInvalidCodeFormatError()
       : base("OrganizationInvalidCodeFormatError", "Code must contain only letters and number.") { }
}

public class OrganizationInvalidTypeError : ErrorMessage
{
   public OrganizationInvalidTypeError()
       : base("OrganizationInvalidTypeError", "Invalid Type, inform 1 for a Company and 2 for an Individual.") { }
}