namespace Shared.Domain;

public static partial class SharedConst
{
   public static class Database
   {
      public const string ConnectionString = "SharedDb";
      public const string Schema = "shared";

      public const string TextType = "text";
      public const string UuidType = "uuid";
   }

   public static class Entity
   {
      public const string Parameter = nameof(Entities.Parameter);
      public const string ParameterOverride = nameof(Entities.ParameterOverride);
   }

   public static class Redis
   {
      public const string ConnectionString = "Redis";

      public const string AuditLogEventsStream = "audit-log-events";
      public const string SystemLogEventsStream = "system-log-events";
      public const string NotificationEventsChannel = "notification-events";
   }

   public static class System
   {
      public const string ModuleName = "Shared";
   }
}
