using Myce.Response.Messages;

namespace IAM.Domain.Messages.Errors;

public class DuplicateCustomerCodeError : ErrorMessage
{
   public DuplicateCustomerCodeError(string code)
       : base("DuplicateCodeError", "The customer code '{code}' already exists.")
   {
      AddVariable("code", code);
   }
}

public class InvalidCustomerCodeFormatError : ErrorMessage
{
   public InvalidCustomerCodeFormatError()
       : base("InvalidCodeFormatError", "Code must contain only letters and number.") { }
}

public class InvalidCustomerTypeError : ErrorMessage
{
   public InvalidCustomerTypeError()
       : base("InvalidCustomerTypeError", "Invalid Type, inform 1 for a Company and 2 for an Individual.") { }
}