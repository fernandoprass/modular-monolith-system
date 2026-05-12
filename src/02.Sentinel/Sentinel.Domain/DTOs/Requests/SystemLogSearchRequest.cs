using Shared.Domain.Enums;

namespace Sentinel.Domain.DTOs.Requests;

public record SystemLogSearchRequest(
   Guid? OrganizationId,
   Guid? UserId,
   SystemLogLevel? Level,
   SystemLogStatus? Status,
   string? Source,
   string? RequestId,
   DateTime? From,
   DateTime? To,
   int PageNumber = 1,
   int PageSize = 50);
