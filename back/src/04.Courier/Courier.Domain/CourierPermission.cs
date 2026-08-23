namespace Courier.Domain;

public static class CourierPermission
{
   private const string Module = "courier";

   public static class Emails
   {
      private const string Resource = "emails";

      public const string Read = $"{Module}.{Resource}.read";
      public const string Write = $"{Module}.{Resource}.write";
   }

   public static class Templates
   {
      private const string Resource = "templates";

      public const string Read = $"{Module}.{Resource}.read";
      public const string Write = $"{Module}.{Resource}.write";
   }

   public static class Notifications
   {
      private const string Resource = "notifications";

      public const string Read = $"{Module}.{Resource}.read";
      public const string Write = $"{Module}.{Resource}.write";
   }

   public static class UserPreferences
   {
      private const string Resource = "userpreferences";

      public const string Read = $"{Module}.{Resource}.read";
      public const string Write = $"{Module}.{Resource}.write";
   }
}
