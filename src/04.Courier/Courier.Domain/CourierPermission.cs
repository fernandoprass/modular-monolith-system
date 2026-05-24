namespace Courier.Domain;

public static class CourierPermission
{
   private const string Module = "courier";

   public static class Emails
   {
      private const string Resource = "emails";

      public const string List = $"{Module}.{Resource}.list";
      public const string View = $"{Module}.{Resource}.view";
      public const string Create = $"{Module}.{Resource}.create";
   }

   public static class Templates
   {
      private const string Resource = "templates";

      public const string List = $"{Module}.{Resource}.list";
      public const string View = $"{Module}.{Resource}.view";
      public const string Create = $"{Module}.{Resource}.create";
      public const string Update = $"{Module}.{Resource}.update";
      public const string Delete = $"{Module}.{Resource}.delete";
   }
}
