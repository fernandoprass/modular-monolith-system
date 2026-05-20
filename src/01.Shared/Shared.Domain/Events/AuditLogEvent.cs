using Shared.Domain.Enums;

namespace Shared.Domain.Events;

public record AuditLogEvent
{
   public Guid Id { get; init; } = Guid.CreateVersion7();
   public Guid OrganizationId { get; init; }
   public DateTime Timestamp { get; init; } = DateTime.UtcNow;
   public string Module { get; init; } = string.Empty;
   public string Feature { get; init; } = string.Empty;
   public string Action { get; init; } = string.Empty;
   public AuditPrivacyLevel PrivacyLevel { get; init; } = AuditPrivacyLevel.Medium;
   public string Description { get; init; } = string.Empty;
   public Guid UserId { get; init; }
   public Guid TargetId { get; init; }
   public string? IpAddress { get; init; }
   public string? UserAgent { get; init; }
   public string Metadata { get; init; } = "{}";
}
