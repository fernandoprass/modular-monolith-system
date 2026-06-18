using Shared.Domain.Enums;

namespace Sentinel.Domain.DTOs.Responses;

public record AuditLogDto(
   Guid Id,
   string Module,
   string Feature,
   string Action,
   AuditPrivacyLevel PrivacyLevel,
   string Description,
   DateTime CreatedAt,
   DateTime ExpiresAt,
   Guid UserId,
   Guid OrganizationId,
   Guid TargetId,
   string? IpAddress,
   string? UserAgent,
   string Metadata);
