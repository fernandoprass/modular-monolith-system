namespace Courier.Domain.DTOs.Requests;

public record EmailSearchRequest(
   Guid? OrganizationId,
   Guid? UserId,
   string? Module,
   string? Feature,
   string? Subject,
   string? Recipient,
   DateTime DateFrom,
   DateTime DateTo,
   int PageNumber = 1,
   int PageSize = 25);
