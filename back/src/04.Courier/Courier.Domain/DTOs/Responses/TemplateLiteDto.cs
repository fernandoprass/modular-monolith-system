using Courier.Domain.Enums;
using Shared.Domain.Enums;

namespace Courier.Domain.DTOs.Responses;

public record TemplateLiteDto(
   Guid Id,
   string Module,
   string Key,
   string Name,
   bool IsAllowingOptOut,
   NotificationSeverity Severity,
   RetentionPolicy RetentionPolicy);
