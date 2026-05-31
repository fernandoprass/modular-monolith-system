namespace IAM.Domain;

public static class IamPermission
{
   private const string Module = "iam";

   public static class Organizations
   {
      private const string Resource = "organizations";

      public const string List = $"{Module}.{Resource}.list";
      public const string View = $"{Module}.{Resource}.view";
      public const string Create = $"{Module}.{Resource}.create";
      public const string Update = $"{Module}.{Resource}.update";
      public const string Delete = $"{Module}.{Resource}.delete";
   }

   public static class Users
   {
      private const string Resource = "users";

      public const string List = $"{Module}.{Resource}.list";
      public const string View = $"{Module}.{Resource}.view";
      public const string Create = $"{Module}.{Resource}.create";
      public const string Update = $"{Module}.{Resource}.update";
      public const string UpdateOrganizationAdmin = $"{Module}.{Resource}.updateorganizationadmin";
      public const string Delete = $"{Module}.{Resource}.delete";
   }

   public static class Roles
   {
      private const string Resource = "roles";

      public const string List = $"{Module}.{Resource}.list";
      public const string View = $"{Module}.{Resource}.view";
      public const string Create = $"{Module}.{Resource}.create";
      public const string Update = $"{Module}.{Resource}.update";
      public const string Assign = $"{Module}.{Resource}.assign";
      public const string ViewPermissions = $"{Module}.{Resource}.viewpermissions";
   }

   public static class Parameters
   {
      private const string Resource = "parameters";

      public const string List = $"{Module}.{Resource}.list";
      public const string View = $"{Module}.{Resource}.view";
      public const string Update = $"{Module}.{Resource}.update";
      public const string SaveOverride = $"{Module}.{Resource}.saveoverride";
      public const string DeleteOverride = $"{Module}.{Resource}.deleteoverride";
   }

   public static class Permissions
   {
      private const string Resource = "permissions";

      public const string List = $"{Module}.{Resource}.list";
      public const string Update = $"{Module}.{Resource}.update";
      public const string Assign = $"{Module}.{Resource}.assign";
   }
}
