using Shared.Domain.Enums;

namespace Sentinel.Domain.DTOs.Requests;

public record AuditLogSearchRequest(
   Guid? OrganizationId,
   Guid? UserId,
   string? Module,
   string? Feature,
   string? Action,
   AuditPrivacyLevel? PrivacyLevel,
   Guid? TargetId,
   DateTime? From,
   DateTime? To,
   int PageNumber = 1,
   int PageSize = 50);
