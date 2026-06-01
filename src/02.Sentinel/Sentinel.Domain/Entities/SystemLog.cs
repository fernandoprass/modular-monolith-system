using Shared.Domain.Entities;
using Shared.Domain.Enums;

namespace Sentinel.Domain.Entities;

public class SystemLog : Entity
{
   public SystemLogLevel Level { get; private set; }
   public SystemLogStatus Status { get; private set; } = SystemLogStatus.Unknown;
   public string Source { get; private set; } = string.Empty;
   public string Message { get; private set; } = string.Empty;
   public string? Exception { get; private set; }
   public string? StackTrace { get; private set; }
   public string? RequestId { get; private set; }
   public DateTime CreatedAt { get; private set; }
   public DateTime ExpiresAt { get; private set; }
   public Guid? UserId { get; private set; }
   public Guid? OrganizationId { get; private set; }
   public string PropertiesJson { get; private set; } = "{}";

   private SystemLog() { }

   public static SystemLog Create(
      Guid id,
      SystemLogLevel level,
      SystemLogStatus status,
      RetentionPolicy retentionPolicy,
      string source,
      string message,
      string? exception,
      string? stackTrace,
      string? requestId,
      Guid? userId,
      Guid? organizationId,
      string propertiesJson)
   {
      var now = DateTime.UtcNow;

      return new SystemLog
      {
         Id = id,
         Level = level,
         Status = status,
         Source = source,
         Message = message,
         Exception = exception,
         StackTrace = stackTrace,
         RequestId = requestId,
         CreatedAt = now,
         ExpiresAt = now.AddDays(GetRetentionDays(retentionPolicy)),
         UserId = userId,
         OrganizationId = organizationId,
         PropertiesJson = string.IsNullOrWhiteSpace(propertiesJson) ? "{}" : propertiesJson
      };
   }

   private static int GetRetentionDays(RetentionPolicy retentionPolicy)
   {
      return retentionPolicy switch
      {
         RetentionPolicy.Operational => SentinelConst.RetentionPoliciesTimeSpans.SystemLog.Operational,
         RetentionPolicy.Standard => SentinelConst.RetentionPoliciesTimeSpans.SystemLog.Standard,
         RetentionPolicy.Extended => SentinelConst.RetentionPoliciesTimeSpans.SystemLog.Extended,
         RetentionPolicy.Compliance => SentinelConst.RetentionPoliciesTimeSpans.SystemLog.Compliance,
         RetentionPolicy.LongTerm => SentinelConst.RetentionPoliciesTimeSpans.SystemLog.LongTerm,
         _ => SentinelConst.RetentionPoliciesTimeSpans.SystemLog.Standard
      };
   }
}
