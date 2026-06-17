namespace IAM.Domain.DTOs.Requests;

public sealed record UserSearchRequest(
   Guid OrganizationId,
   string? Name,
   string? Email,
   bool? IsActive = null,
   int PageNumber = 1,
   int PageSize = 25
);