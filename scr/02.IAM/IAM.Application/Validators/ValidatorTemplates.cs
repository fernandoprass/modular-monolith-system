using IAM.Domain.Messages;
using Myce.FluentValidator;

namespace IAM.Application.Validators;

public static class ValidatorTemplates
{
   public static void NameRules<T>(RuleBuilder<T, string> rb) where T : class
      => rb.IsRequired().MinLength(3);

   public static void EmailRules<T>(RuleBuilder<T, string> rb) where T : class
      => rb.IsRequired().IsValidEmailAddress();

   public static void PasswordRules<T>(RuleBuilder<T, string> rb) where T : class
   {
      // 1. Has minimum 8 characters in length
      rb.MinLength(8, new PasswordMinLengthError());

      // 2. At least one uppercase English letter
      rb.Custom(pwd => !string.IsNullOrEmpty(pwd) && pwd.Any(char.IsUpper), new PasswordMissingUppercaseError());

      // 3. At least one lowercase English letter
      rb.Custom(pwd => !string.IsNullOrEmpty(pwd) && pwd.Any(char.IsLower), new PasswordMissingLowercaseError());

      // 4. At least one digit
      rb.Custom(pwd => !string.IsNullOrEmpty(pwd) && pwd.Any(char.IsDigit), new PasswordMissingDigitError());

      // 5. At least one special character
      // We define special characters manually to match your previous requirement: #?!@$%^&*-
      char[] specialChars = { '#', '?', '!', '@', '$', '%', '^', '&', '*', '-', '_', '.' };
      rb.Custom(pwd => !string.IsNullOrEmpty(pwd) && pwd.Any(c => specialChars.Contains(c)), new PasswordMissingSpecialError());
   }
}
