namespace IAM.Domain;

public static partial class IamConst
{
   public static class Organization
   {
      public const byte RandomCodeSize = 10;
   }

   public static class Database
   {
      public const string ConnectionString = "IamDb";
      public const string Schema = "iam";
   }

   public static class Entity
   {
      public const string Organization = nameof(Entities.Organization);
      public const string User = nameof(Entities.User);
      public const string Role = nameof(Entities.Role);
      public const string Permission = nameof(Entities.Permission);
   }

   public static class Logger
   {
      public static class Feature
      {
         public const string Authentication = "authentication";
         public const string Organizations = "organizations";
         public const string Permissions = "permissions";
         public const string Roles = "roles";
         public const string Users = "users";
      }

      public static class Action
      {
         public const string Create = "create";
         public const string Update = "update";
         public const string UpdateCode = "update-code";
         public const string UpdatePassword = "update-password";
         public const string UpdateOrganizationAdmin = "update-organization-admin";
         public const string Delete = "delete";
         public const string LoginSuccess = "login-success";
         public const string LoginFail = "login-fail";
         public const string Assign = "assign";
         public const string Unassign = "unassign";
      }
   }

   public static class EmailTemplate
   {
      public const string OrganizationWelcome = "orgazination-welcome";
      public const string OrganizationDelete = "orgazination-delete";
      public const string UserWelcome = "user-welcome";
      public const string UserPasswordUpdated = "user-password-updated";
      public const string UserResetPassword = "user-reset-password";
      public const string UserMaxFailedLoginAttempts = "user-max-failed-login-attempts";
      public const string UserDelete = "user-delete";
   }

   public static class System
   {
      public const string ModuleName = "IAM";
   }
}
