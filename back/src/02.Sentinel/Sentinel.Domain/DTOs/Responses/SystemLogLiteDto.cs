using Shared.Domain.Enums;

namespace Sentinel.Domain.DTOs.Responses;

public record SystemLogLiteDto(
   Guid Id,
   SystemLogLevel Level,
   SystemLogStatus Status,
   string Module,
   string Message,
   DateTime CreatedAt);
