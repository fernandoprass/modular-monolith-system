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
}
