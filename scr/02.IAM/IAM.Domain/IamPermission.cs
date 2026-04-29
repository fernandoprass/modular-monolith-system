namespace IAM.Domain;

public static class IamPermission
{
   private const string Module = "iam";

   public static class Organizations
   {
      private const string Group = "organizations";

      public const string List = $"{Module}.{Group}.list";
      public const string View = $"{Module}.{Group}.view";
      public const string Create = $"{Module}.{Group}.create";
      public const string Update = $"{Module}.{Group}.update";
      public const string Delete = $"{Module}.{Group}.delete";
   }

   public static class Users
   {
      private const string Group = "users";

      public const string List = $"{Module}.{Group}.list";
      public const string View = $"{Module}.{Group}.view";
      public const string Create = $"{Module}.{Group}.create";
      public const string Update = $"{Module}.{Group}.update";
      public const string Delete = $"{Module}.{Group}.delete";
   }

   public static class Roles
   {
      private const string Group = "roles";

      public const string List = $"{Module}.{Group}.list";
      public const string View = $"{Module}.{Group}.view";
      public const string Create = $"{Module}.{Group}.create";
      public const string Update = $"{Module}.{Group}.update";
      public const string Assign = $"{Module}.{Group}.assign";
      public const string ViewPermissions = $"{Module}.{Group}.viewpermissions";
   }

   public static class Parameters
   {
      private const string Group = "parameters";

      public const string List = $"{Module}.{Group}.list";
      public const string View = $"{Module}.{Group}.view";
      public const string SaveOverride = $"{Module}.{Group}.saveoverride";
      public const string DeleteOverride = $"{Module}.{Group}.deleteoverride";
   }

   public static class Permissions
   {
      private const string Group = "permissions";

      public const string List = $"{Module}.{Group}.list";
      public const string Create = $"{Module}.{Group}.create";
      public const string Update = $"{Module}.{Group}.update";
      public const string Delete = $"{Module}.{Group}.delete";
      public const string Assign = $"{Module}.{Group}.assign";
   }
}
