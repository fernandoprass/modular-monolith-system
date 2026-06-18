using System.Text.RegularExpressions;

namespace IAM.Domain;

public static class IamParam
{
   private const string Module = "IAM";

   public static class Security
   {
      private const string Group = "Security";

      public const string MaxPasswordAgeInDays = $"{Module}.{Group}.{nameof(MaxPasswordAgeInDays)}";
      public const string LockoutDurationInMins = $"{Module}.{Group}.{nameof(LockoutDurationInMins)}";
      public const string MaxFailedLoginAttempts = $"{Module}.{Group}.{nameof(MaxFailedLoginAttempts)}";
      public const string JwtExpirationInHours = $"{Module}.{Group}.{nameof(JwtExpirationInHours)}";
   }

   public static class Role
   {
      private const string Group = "Role";

      public const string DefaultRoleIdForNewUser = $"{Module}.{Group}.{nameof(DefaultRoleIdForNewUser)}";
      public const string DefaultRoleIdForNewOrganization = $"{Module}.{Group}.{nameof(DefaultRoleIdForNewOrganization)}";
   }
}
