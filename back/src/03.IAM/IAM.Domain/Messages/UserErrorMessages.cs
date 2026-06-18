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

public class UnauthorizedAccessError : ErrorMessage
{
   public UnauthorizedAccessError() : base("UnauthorizedAccessError", "Unauthorized access.") { }
}

public class InvalidEmailPasswordError : ErrorMessage
{
   public InvalidEmailPasswordError() : base("InvalidEmailPasswordError", "Invalid email or password.") { }
}

public class AccountLockedError : ErrorMessage
{
   public AccountLockedError(int minutesRemaining) :
      base("AccountLockedError", "Account is locked due to too many failed login attempts. Try again in {MinutesRemaining} minute(s).")
   {
      AddVariable("MinutesRemaining", minutesRemaining.ToString());
   }
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
   public PasswordMissingSpecialError() : base("PasswordMissingSpecialError", "Password must contain at least one special character (#?!@$%^&*-_.).") { }
}
