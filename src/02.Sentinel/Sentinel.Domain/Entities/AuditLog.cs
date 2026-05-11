using Shared.Domain.Entities;
using Shared.Domain.Enums;

namespace Sentinel.Domain.Entities;

public class AuditLog : Entity
{
   public DateTime Timestamp { get; private set; }
   public string Module { get; private set; } = string.Empty;
   public string Feature { get; private set; } = string.Empty;
   public string Action { get; private set; } = string.Empty;
   public AuditPrivacyLevel PrivacyLevel { get; private set; } = AuditPrivacyLevel.Medium;
   public string Description { get; private set; } = string.Empty;
   public Guid UserId { get; private set; }
   public Guid OrganizationId { get; private set; }
   public string Entity { get; private set; } = string.Empty;
   public Guid EntityId { get; private set; }
   public string? IpAddress { get; private set; }
   public string? UserAgent { get; private set; }
   public string Metadata { get; private set; } = "{}";

   private AuditLog() { }

   public static AuditLog Create(
      DateTime timestamp,
      string module,
      string feature,
      string action,
      AuditPrivacyLevel privacyLevel,
      string description,
      Guid userId,
      Guid organizationId,
      string entity,
      Guid entityId,
      string? ipAddress,
      string? userAgent,
      string metadata)
   {
      return new AuditLog
      {
         Id = Guid.CreateVersion7(),
         Timestamp = timestamp,
         Module = module,
         Feature = feature,
         Action = action,
         PrivacyLevel = privacyLevel,
         Description = description,
         UserId = userId,
         OrganizationId = organizationId,
         Entity = entity,
         EntityId = entityId,
         IpAddress = ipAddress,
         UserAgent = userAgent,
         Metadata = string.IsNullOrWhiteSpace(metadata) ? "{}" : metadata
      };
   }
}
