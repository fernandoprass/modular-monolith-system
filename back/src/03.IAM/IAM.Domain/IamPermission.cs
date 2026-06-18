namespace IAM.Domain;

public static class IamPermission
{
   private const string Module = "iam";

   public static class Organizations
   {
      private const string Resource = "organizations";
      public const string Read = $"{Module}.{Resource}.read";
      public const string Write = $"{Module}.{Resource}.write";
   }

   public static class OrganizationProfile
   {
      private const string Resource = "organizationprofile";
      public const string Read = $"{Module}.{Resource}.read";
      public const string Write = $"{Module}.{Resource}.write";
      public const string Delete = $"{Module}.{Resource}.delete";
      public const string Parameters = $"{Module}.{Resource}.parameters";
   }

   public static class Parameters
   {
      private const string Resource = "parameters";
      public const string Read = $"{Module}.{Resource}.read";
      public const string Write = $"{Module}.{Resource}.write";
      public const string Override = $"{Module}.{Resource}.override";
   }

   public static class Permissions
   {
      private const string Resource = "permissions";
      public const string Read = $"{Module}.{Resource}.read";
      public const string Write = $"{Module}.{Resource}.write";
      public const string Assign = $"{Module}.{Resource}.assign";
   }

   public static class Roles
   {
      private const string Resource = "roles";
      public const string Read = $"{Module}.{Resource}.read";
      public const string Write = $"{Module}.{Resource}.write";
      public const string Assign = $"{Module}.{Resource}.assign";
   }

   public static class Users
   {
      private const string Resource = "users";
      public const string Read = $"{Module}.{Resource}.read";
      public const string Write = $"{Module}.{Resource}.write";
      public const string ViewAccess = $"{Module}.{Resource}.viewaccess";
      public const string UpdateSupportUser = $"{Module}.{Resource}.updatesupportuser";
      public const string UpdateOrganizationAdmin = $"{Module}.{Resource}.updateorganizationadmin";
   }

   public static class UserProfile
   {
      private const string Resource = "userprofile";
      public const string Read = $"{Module}.{Resource}.read";
      public const string Write = $"{Module}.{Resource}.write";
      public const string Delete = $"{Module}.{Resource}.delete";
      public const string ViewAccess = $"{Module}.{Resource}.viewaccess";
      public const string Parameters = $"{Module}.{Resource}.parameters";
   }
}
