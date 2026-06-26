using Courier.Domain.Enums;
using Shared.Domain.Enums;

namespace Courier.Domain.DTOs.Requests;

public record TemplateCreateRequest(
   string Module,
   string Key,
   bool IsAllowingOptOut,
   NotificationSeverity Severity,
   RetentionPolicy RetentionPolicy);
