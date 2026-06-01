using Shared.Domain.Enums;

namespace Shared.Domain.Events;

public record AuditLogEvent
{
   public Guid Id { get; init; } = Guid.CreateVersion7();
   public string Module { get; init; } = string.Empty;
   public string Feature { get; init; } = string.Empty;
   public string Action { get; init; } = string.Empty;
   public string Description { get; init; } = string.Empty;
   public AuditPrivacyLevel PrivacyLevel { get; init; } = AuditPrivacyLevel.Medium;
   public RetentionPolicy RetentionPolicy { get; init; } = RetentionPolicy.Standard;
   public string? IpAddress { get; init; }
   public string? UserAgent { get; init; }
   public Guid UserId { get; init; }
   public Guid TargetId { get; init; }
   public Guid OrganizationId { get; init; }
   public string Metadata { get; init; } = "{}";

   private AuditLogEvent() { }

   public static AuditLogEvent Create(
      string module, 
      string feature, 
      string action, 
      string description,
      AuditPrivacyLevel privacyLevel,
      RetentionPolicy retentionPolicy,
      string? ipAddress,
      string? userAgent,
      Guid userId,
      Guid targetId,
      Guid organizationId,
      string metadata
      )
   {
      return new AuditLogEvent
      {
         Id = Guid.CreateVersion7(),
         Module = module,
         Feature = feature,
         Action = action,
         Description = description,
         PrivacyLevel = privacyLevel,
         RetentionPolicy = retentionPolicy,
         IpAddress = ipAddress,
         UserAgent = userAgent,
         UserId = userId,
         TargetId = targetId,
         OrganizationId = organizationId,
         Metadata = metadata
      };
   }
}
