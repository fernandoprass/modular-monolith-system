namespace Shared.Domain;

public static class SharedPermission
{
   private const string Module = "shared";

   public static class Lookups
   {
      private const string Resource = "lookups";
      public const string Read = $"{Module}.{Resource}.read";
      public const string Create = $"{Module}.{Resource}.create";
      public const string Update = $"{Module}.{Resource}.update";
      public const string Delete = $"{Module}.{Resource}.delete";
   }
}
