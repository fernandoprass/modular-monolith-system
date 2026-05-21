using Shared.Domain.Entities;
using Shared.Domain.Enums;

namespace Sentinel.Domain.Entities;

public class AuditLog : Entity
{
   public DateTime CreatedAt { get; private set; }
   public string Module { get; private set; } = string.Empty;
   public string Feature { get; private set; } = string.Empty;
   public string Action { get; private set; } = string.Empty;
   public AuditPrivacyLevel PrivacyLevel { get; private set; } = AuditPrivacyLevel.Medium;
   public string Description { get; private set; } = string.Empty;
   public Guid UserId { get; private set; }
   public Guid OrganizationId { get; private set; }
   public Guid TargetId { get; private set; }
   public string? IpAddress { get; private set; }
   public string? UserAgent { get; private set; }
   public string Metadata { get; private set; } = "{}";

   private AuditLog() { }

   public static AuditLog Create(
      Guid id,
      DateTime createdAt,
      string module,
      string feature,
      string action,
      AuditPrivacyLevel privacyLevel,
      string description,
      Guid userId,
      Guid organizationId,
      Guid targetId,
      string? ipAddress,
      string? userAgent,
      string metadata)
   {
      return new AuditLog
      {
         Id = id,
         CreatedAt = createdAt,
         Module = module,
         Feature = feature,
         Action = action,
         PrivacyLevel = privacyLevel,
         Description = description,
         UserId = userId,
         OrganizationId = organizationId,
         TargetId = targetId,
         IpAddress = ipAddress,
         UserAgent = userAgent,
         Metadata = string.IsNullOrWhiteSpace(metadata) ? "{}" : metadata
      };
   }
}
