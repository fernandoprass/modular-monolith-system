using Shared.Domain;

namespace Sentinel.Domain;

public static partial class SentinelConst
{
   public static class Database
   {
      public const string ConnectionString = "SentinelDb";
      public const string Schema = "sentinel";
      public const string TextType = "text";
      public const string JsonbType = "jsonb";
      public const string UuidType = "uuid";
   }

   public static class Entity
   {
      public const string AuditLog = nameof(Entities.AuditLog);
      public const string SystemLog = nameof(Entities.SystemLog);
   }

   public static class System
   {
      public const string ModuleName = "Sentinel";
   }

   public static class Redis
   {
      public const string AuditLogEventsStream = SharedConst.Redis.AuditLogEventsStream;
      public const string SystemLogEventsStream = SharedConst.Redis.SystemLogEventsStream;
      public const string NotificationEventsChannel = SharedConst.Redis.NotificationEventsChannel;

      public const string EventFieldName = "event";
      public const string InitialStreamPosition = "0-0";

      public const string AuditConsumerGroup = "sentinel-audit-consumer";
      public const string SystemLogConsumerGroup = "sentinel-system-log-consumer";

      public const string AuditConsumerNamePrefix = "sentinel-audit";
      public const string SystemLogConsumerNamePrefix = "sentinel-system-log";

      public const int ReadBatchSize = 10;
      public const int EmptyStreamDelaySeconds = 1;
      public const int ErrorDelaySeconds = 5;
   }
}
