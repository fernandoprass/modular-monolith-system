using Shared.Domain.Enums;

namespace Sentinel.Domain.DTOs.Responses;

public record SystemLogDto(
   Guid Id,
   SystemLogLevel Level,
   SystemLogStatus Status,
   string Module,
   string Message,
   string? Exception,
   string? StackTrace,
   DateTime CreatedAt,
   DateTime ExpiresAt,
   string? RequestId,
   Guid? UserId,
   Guid? OrganizationId,
   string PropertiesJson);
