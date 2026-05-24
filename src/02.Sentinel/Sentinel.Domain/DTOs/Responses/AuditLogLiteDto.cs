using Shared.Domain.Enums;

namespace Sentinel.Domain.DTOs.Responses;

public record AuditLogLiteDto(
   Guid Id,
   DateTime CreatedAt,
   string Module,
   string Feature,
   string Action,
   AuditPrivacyLevel PrivacyLevel,
   string Description,
   Guid UserId,
   Guid TargetId);
