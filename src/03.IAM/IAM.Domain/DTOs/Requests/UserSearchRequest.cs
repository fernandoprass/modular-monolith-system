namespace IAM.Domain.DTOs.Requests;

public sealed record UserSearchRequest(
   string? Name,
   string? Email,
   bool? IsActive = null,
   Guid? OrganizationId = null,
   int PageNumber = 1,
   int PageSize = 25
);