using Courier.Domain.Enums;

namespace Courier.Domain.DTOs.Requests;

public record NotificationSearchRequest(
   Guid? OrganizationId,
   Guid? UserId,
   string? Module,
   string? Title,
   NotificationStatus? Status,
   DateTime DateFrom,
   DateTime DateTo,
   int PageNumber = 1,
   int PageSize = 25);
