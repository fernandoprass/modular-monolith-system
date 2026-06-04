using Shared.Domain.Enums;

namespace Sentinel.Domain.DTOs.Responses;

public record SystemLogLiteDto(
   Guid Id,
   SystemLogLevel Level,
   SystemLogStatus Status,
   string Module,
   string Message,
   DateTime CreatedAt,
   DateTime ExpiresAt,
   string? RequestId,
   Guid? UserId,
   Guid? OrganizationId);
