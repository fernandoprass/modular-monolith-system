namespace Sentinel.Domain;

public static class SentinelPermission
{
   private const string Module = "sentinel";

   public static class AuditLogs
   {
      private const string Resource = "auditlogs";

      public const string List = $"{Module}.{Resource}.list";
      public const string View = $"{Module}.{Resource}.view";
   }

   public static class SystemLogs
   {
      private const string Resource = "systemlogs";

      public const string List = $"{Module}.{Resource}.list";
      public const string View = $"{Module}.{Resource}.view";
   }
}
