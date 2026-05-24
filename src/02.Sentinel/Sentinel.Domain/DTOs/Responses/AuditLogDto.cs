using Shared.Domain.Enums;

namespace Sentinel.Domain.DTOs.Responses;

public record AuditLogDto(
   Guid Id,
   DateTime CreatedAt,
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
