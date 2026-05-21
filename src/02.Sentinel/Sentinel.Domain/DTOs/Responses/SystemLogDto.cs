using Shared.Domain.Enums;

namespace Sentinel.Domain.DTOs.Responses;

public record SystemLogDto(
   Guid Id,
   DateTime CreatedAt,
   SystemLogLevel Level,
   SystemLogStatus Status,
   string Source,
   string Message,
   string? Exception,
   string? StackTrace,
   string? RequestId,
   Guid? UserId,
   Guid? OrganizationId,
   string PropertiesJson);
