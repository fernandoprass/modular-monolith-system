using Myce.Response.Messages;

namespace IAM.Domain.Messages;

public class EmailAlreadyExistError : ErrorMessage
{
   public EmailAlreadyExistError(string email)
     : base("EmailAlreadyExistError", "The email '{email}' already exist.")
   {
      AddVariable("email", email);
   }
}

public class OrganizationForbiddenError : ErrorMessage
{
   public OrganizationForbiddenError() : base("OrganizationForbiddenError", "The informing organization is different from the logged-in organization.") { }
}

public class PasswordNotValidError : ErrorMessage
{
   public PasswordNotValidError() : base("EmailAlreadyExistError", "The password is not valid.") { }
}

public class PasswordMinLengthError : ErrorMessage
{
   public PasswordMinLengthError() : base("PasswordMinLengthError", "Password must contain at least eight letters.") { }
}

public class PasswordMissingUppercaseError : ErrorMessage
{
   public PasswordMissingUppercaseError() : base("PasswordMissingUppercaseError", "Password must contain at least one uppercase letter.") { }
}

public class PasswordMissingLowercaseError : ErrorMessage
{
   public PasswordMissingLowercaseError() : base("PasswordMissingLowercaseError", "Password must contain at least one lowercase letter.") { }
}

public class PasswordMissingDigitError : ErrorMessage
{
   public PasswordMissingDigitError() : base("PasswordMissingDigitError", "Password must contain at least one digit.") { }
}

public class PasswordMissingSpecialError : ErrorMessage
{
   public PasswordMissingSpecialError() : base("UserPasswordMinLengthError", "Password must contain at least one special character (#?!@$%^&*-_.).") { }
}
