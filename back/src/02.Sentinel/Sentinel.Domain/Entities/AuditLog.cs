using Shared.Domain.Entities;
using Shared.Domain.Enums;

namespace Sentinel.Domain.Entities;

public class AuditLog : Entity
{
   public string Module { get; private set; } = string.Empty;
   public string Feature { get; private set; } = string.Empty;
   public string Action { get; private set; } = string.Empty;
   public string Description { get; private set; } = string.Empty;
   public AuditPrivacyLevel PrivacyLevel { get; private set; } = AuditPrivacyLevel.Medium;
   public string? IpAddress { get; private set; }
   public string? UserAgent { get; private set; }
   public DateTime CreatedAt { get; private set; }
   public DateTime ExpiresAt { get; private set; }
   public Guid UserId { get; private set; }
   public Guid OrganizationId { get; private set; }
   public Guid TargetId { get; private set; }
   public string Metadata { get; private set; } = "{}";

   private AuditLog() { }

   public static AuditLog Create(
      Guid id,
      string module,
      string feature,
      string action,
      string description,
      AuditPrivacyLevel privacyLevel,
      RetentionPolicy retentionPolicy,
      string? ipAddress,
      string? userAgent,
      Guid userId,
      Guid organizationId,
      Guid targetId,
      string metadata)
   {
      var now = DateTime.UtcNow;

      return new AuditLog
      {
         Id = id,
         Module = module,
         Feature = feature,
         Action = action,
         Description = description,
         PrivacyLevel = privacyLevel,
         IpAddress = ipAddress,
         UserAgent = userAgent,
         CreatedAt = now,
         ExpiresAt = now.AddDays(GetRetentionDays(retentionPolicy)),
         UserId = userId,
         OrganizationId = organizationId,
         TargetId = targetId,
         Metadata = string.IsNullOrWhiteSpace(metadata) ? "{}" : metadata
      };
   }

   private static int GetRetentionDays(RetentionPolicy retentionPolicy)
   {
      return retentionPolicy switch
      {
         RetentionPolicy.Operational => SentinelConst.RetentionPoliciesTimeSpans.AuditLog.Operational,
         RetentionPolicy.Standard => SentinelConst.RetentionPoliciesTimeSpans.AuditLog.Standard,
         RetentionPolicy.Extended => SentinelConst.RetentionPoliciesTimeSpans.AuditLog.Extended,
         RetentionPolicy.Compliance => SentinelConst.RetentionPoliciesTimeSpans.AuditLog.Compliance,
         RetentionPolicy.LongTerm => SentinelConst.RetentionPoliciesTimeSpans.AuditLog.LongTerm,
         _ => SentinelConst.RetentionPoliciesTimeSpans.AuditLog.Standard
      };
   }
}
