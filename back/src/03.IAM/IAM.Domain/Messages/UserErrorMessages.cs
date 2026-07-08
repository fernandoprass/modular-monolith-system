using Myce.Response.Messages;

namespace IAM.Domain.Messages;

public class EmailAlreadyExistError : ErrorMessage
{
   public EmailAlreadyExistError(string email)
      : base(
         IamTranslatedMessagesProvider.EmailAlreadyExistError,
         IamTranslatedMessagesProvider.Instance.GetTranslations(IamTranslatedMessagesProvider.EmailAlreadyExistError))
   {
      AddVariable("email", email);
   }
}

public class UnauthorizedAccessError : ErrorMessage
{
   public UnauthorizedAccessError()
      : base(
         IamTranslatedMessagesProvider.UnauthorizedAccessError,
         IamTranslatedMessagesProvider.Instance.GetTranslations(IamTranslatedMessagesProvider.UnauthorizedAccessError)) { }
}

public class InvalidEmailPasswordError : ErrorMessage
{
   public InvalidEmailPasswordError()
      : base(
         IamTranslatedMessagesProvider.InvalidEmailPasswordError,
         IamTranslatedMessagesProvider.Instance.GetTranslations(IamTranslatedMessagesProvider.InvalidEmailPasswordError)) { }
}

public class AccountLockedError : ErrorMessage
{
   public AccountLockedError(int minutesRemaining)
      : base(
         IamTranslatedMessagesProvider.AccountLockedError,
         IamTranslatedMessagesProvider.Instance.GetTranslations(IamTranslatedMessagesProvider.AccountLockedError))
   {
      AddVariable("MinutesRemaining", minutesRemaining.ToString());
   }
}

public class PasswordNotValidError : ErrorMessage
{
   public PasswordNotValidError()
      : base(
         IamTranslatedMessagesProvider.PasswordNotValidError,
         IamTranslatedMessagesProvider.Instance.GetTranslations(IamTranslatedMessagesProvider.PasswordNotValidError)) { }
}

public class PasswordMinLengthError : ErrorMessage
{
   public PasswordMinLengthError()
      : base(
         IamTranslatedMessagesProvider.PasswordMinLengthError,
         IamTranslatedMessagesProvider.Instance.GetTranslations(IamTranslatedMessagesProvider.PasswordMinLengthError)) { }
}

public class PasswordMissingUppercaseError : ErrorMessage
{
   public PasswordMissingUppercaseError()
      : base(
         IamTranslatedMessagesProvider.PasswordMissingUppercaseError,
         IamTranslatedMessagesProvider.Instance.GetTranslations(IamTranslatedMessagesProvider.PasswordMissingUppercaseError)) { }
}

public class PasswordMissingLowercaseError : ErrorMessage
{
   public PasswordMissingLowercaseError()
      : base(
         IamTranslatedMessagesProvider.PasswordMissingLowercaseError,
         IamTranslatedMessagesProvider.Instance.GetTranslations(IamTranslatedMessagesProvider.PasswordMissingLowercaseError)) { }
}

public class PasswordMissingDigitError : ErrorMessage
{
   public PasswordMissingDigitError()
      : base(
         IamTranslatedMessagesProvider.PasswordMissingDigitError,
         IamTranslatedMessagesProvider.Instance.GetTranslations(IamTranslatedMessagesProvider.PasswordMissingDigitError)) { }
}

public class PasswordMissingSpecialError : ErrorMessage
{
   public PasswordMissingSpecialError()
      : base(
         IamTranslatedMessagesProvider.PasswordMissingSpecialError,
         IamTranslatedMessagesProvider.Instance.GetTranslations(IamTranslatedMessagesProvider.PasswordMissingSpecialError)) { }
}
