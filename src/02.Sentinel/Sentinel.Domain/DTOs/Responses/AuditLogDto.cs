using Shared.Domain.Enums;

namespace Sentinel.Domain.DTOs.Responses;

public record AuditLogDto(
   Guid Id,
   DateTime Timestamp,
   string Module,
   string Feature,
   string Action,
   AuditPrivacyLevel PrivacyLevel,
   string Description,
   Guid UserId,
   Guid OrganizationId,
   Guid TargetId,
   string? IpAddress,
   string? UserAgent,
   string Metadata);
