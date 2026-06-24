using Courier.Domain.Enums;

namespace Courier.Domain.DTOs.Requests;

public record TemplateSearchRequest(
   string? Module,
   string? Key,
   string? Name,
   NotificationSeverity? Severity,
   int PageNumber = 1,
   int PageSize = 25);
