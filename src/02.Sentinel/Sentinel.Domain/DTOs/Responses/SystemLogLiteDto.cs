using Shared.Domain.Enums;

namespace Sentinel.Domain.DTOs.Responses;

public record SystemLogLiteDto(
   Guid Id,
   DateTime CreatedAt,
   SystemLogLevel Level,
   SystemLogStatus Status,
   string Module,
   string Message,
   string? RequestId,
   Guid? UserId,
   Guid? OrganizationId);
