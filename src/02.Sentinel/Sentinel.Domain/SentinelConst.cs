using Shared.Domain;

namespace Sentinel.Domain;

public static partial class SentinelConst
{
   public static class Database
   {
      public const string ConnectionString = "SentinelDb";
      public const string DefaultName = "sentinel";
      public const string Schema = "sentinel";
      public const string TextType = "text";
      public const string JsonbType = "jsonb";
      public const string UuidType = "uuid";

      public static class Collection
      {
         public const string AuditLogs = "audit_logs";
         public const string SystemLogs = "system_logs";
      }
   }

   public static class Entity
   {
      public const string AuditLog = nameof(Entities.AuditLog);
      public const string SystemLog = nameof(Entities.SystemLog);
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

   public static class RetentionPoliciesTimeSpans
   {
      public static class AuditLog
      {
         public const int Operational = 365; // 1 year
         public const int Standard = 1095;   // 3 years
         public const int Extended = 1825;   // 5 years
         public const int Compliance = 3965; // 10 years
         public const int LongTerm = 7930;   // 20 years
      }

      public static class SystemLog
      {
         public const int Operational = 30; // 1 month
         public const int Standard = 90;    // 3 months
         public const int Extended = 180;   // 6 months
         public const int Compliance = 365; // 1 year
         public const int LongTerm = 730;   // 2 years
      }
   }

   public static class System
   {
      public const string ModuleName = "Sentinel";
   }
}
