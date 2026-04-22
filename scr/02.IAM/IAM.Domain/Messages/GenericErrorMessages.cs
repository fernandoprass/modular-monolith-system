using Myce.Response.Messages;

namespace IAM.Domain.Messages
{
   public class UnauthorizedError : ErrorMessage
   {
      public UnauthorizedError() : base("UnauthorizedError", "Unauthorized access.") { }

      public UnauthorizedError(int remainingAttempts) : base("UnauthorizedError", "Invalid email or password. {RemainingAttempts} attempts remaining.")
      {
         AddVariable("RemainingAttempts", remainingAttempts.ToString());
      }
   }

   public class AccountLockedError : ErrorMessage
   {
      public AccountLockedError(int minutesRemaining) : 
         base("AccountLockedError", "Account is locked due to too many failed login attempts. Try again in {MinutesRemaining} minute(s).")
      {
         AddVariable("MinutesRemaining", minutesRemaining.ToString());
      }
   }
}
